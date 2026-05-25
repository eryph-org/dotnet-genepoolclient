# Cloud Image Compatibility

Status: **implemented** in `Eryph.GenePool.Model` — the manifest shape and its
validation are in place (geneset manifest version **1.2**). The resolve flow
(capability ↔ generation checks, transform/upload) remains the consumer's
responsibility, as noted under *Open points*.

## Goal

eryph is gaining the ability to build catlets on public clouds, starting with
**Azure** and **AWS** (`ec2`). A catlet's parent chain ultimately resolves to a
base catlet whose **primary volume** is the OS disk. To run that catlet on a
cloud, eryph needs an OS disk *for that cloud*. There are two ways to obtain one:

1. **Transform path** — convert the eryph genepool VHD to the cloud's format and
   upload it. The result is a normal volume gene, content-addressed and pinned
   like any other, distinguished only by its `arch`
   (`azure/amd64`, `ec2/amd64`, beside the existing `hyperv/amd64`). This already
   fits the current model (`GenesetTagManifestData.VolumeGenes` is an array of
   `GeneReferenceData` keyed by `arch`). Nothing new is required for it; it is the
   fallback that always works and the path for any catlet that diverges from a
   plain base image.

2. **Substitute path** — for many base images the upload is unnecessary: an
   equivalent **vendor / marketplace image** already exists on the cloud (every
   cloud-init-enabled Linux image; Windows on Azure where guest services can be
   injected via a VM extension). Instead of uploading gigabytes, eryph points the
   build at the marketplace image and injects eryph guest services separately.

This document specifies how the **substitute path** is expressed so the resolve
flow knows *how to replace a base catlet's primary volume with a marketplace
image*, flexibly across clouds.

### Why this is not just "another volume"

A marketplace image is **not byte-identical** to the eryph VHD, and vendors
refresh their published images constantly (Azure `WindowsServer` gets new
versions monthly; AWS SSM `.../latest` moves daily). So a marketplace mapping is
**not** a content hash — it is a weaker *compatibility assertion*: "this vendor
image is an acceptable stand-in for this base volume (same OS / edition /
baseline), and eryph guest services are added on top."

Two consequences drive the design:

- **The mapping is mutable and rolls forward.** It must therefore live in the
  **mutable, family-level `geneset.json` (`GenesetManifestData`)** — *not* in the
  hash-pinned tag manifest (`GenesetTagManifestData`). The marketplace equivalent
  of "Windows Server 2022 Standard" is a property of the *family*, not of each
  dated build, and putting it in the immutable tag would force a new tag on every
  vendor refresh. Putting it in `geneset.json` lets it change freely without
  invalidating any pinned tag.
- **No identity is duplicated into the geneset/tag.** What an image *is*
  (`secure_boot`, `tpm` ⇒ generation, OS/edition) is **catlet-model** information
  that already lives in the catlet. The compatibility mapping is purely a
  distribution-layer concern: `drive → candidate cloud images`. The capability ↔
  generation check (don't resolve a TPM/secure-boot catlet onto a Gen1 SKU) is
  performed by the **resolve engine** comparing the catlet's declared
  capabilities against the chosen image — not by storing a copy of the identity.

### Scope of this iteration

- **Base catlets only.** Mixed / customized catlets that diverge from a plain
  base image use the transform/upload path and never need a substitute.
- **Azure + AWS (`ec2`)** reference types. GCP etc. can be added later without
  changing the shape.
- **Roll-forward by default** (Azure `version: latest`, AWS SSM `.../latest`).
  Explicit pinning can be added later as an opt-in.

## Schema

A new optional `cloud_compatibility` member is added to `geneset.json`
(`GenesetManifestData`). It maps each **drive name** to a **flat, ordered array**
of candidate image references:

```jsonc
{
  "geneset": "dbosoft/winsrv2022-standard",
  "public": true,
  "short_description": "Windows Server 2022 Standard",

  "cloud_compatibility": {
    "sda": [
      {
        "cloud": "azure",
        "type": "azure_marketplace",
        "publisher": "MicrosoftWindowsServer",
        "offer": "WindowsServer",
        "sku": "2022-datacenter-g2",
        "version": "latest",
        "guest_services_injection": "extension"
      },
      {
        "cloud": "ec2",
        "type": "ec2_ssm_parameter",
        "parameter": "/aws/service/ami-windows-latest/Windows_Server-2022-English-Full-Base",
        "guest_services_injection": "geneset"
      },
      {
        "cloud": "ec2",
        "type": "ec2_image_filter",
        "owner": "amazon",
        "name_pattern": "Windows_Server-2022-English-Full-Base-*",
        "guest_services_injection": "geneset"
      }
    ]
  }
}
```

### Structure rules

- `cloud_compatibility` is a map of **drive name** (matching a drive in the catlet,
  e.g. `sda`) to an **ordered array** of entries.
- Each entry is **fully self-contained** and identified by two discriminators:
  - `cloud` — one of the known clouds (`azure`, `ec2`), aligned with
    `Hypervisors.KnownNames`. (`ec2` is AWS.)
  - `type` — the reference shape (see below). `cloud` + `type` together select the
    body fields.
- **Order is preference.** Multiple candidates for the same cloud are simply
  consecutive entries; the resolver takes the first that resolves.
- Adding a new cloud or reference type never changes the shape — it is just a new
  `cloud`/`type` value.

### `guest_services_injection`

A vanilla marketplace image does not ship eryph guest services (cloudbase-init
has been removed). This field is a **strategy** telling the build *how* to inject
them; eryph's internal logic owns the concrete mechanics (which extension, which
geneset version):

| Value       | Meaning |
|-------------|---------|
| `extension` | Inject via the cloud's VM-extension model (e.g. a published eryph guest-services Azure VM extension). |
| `geneset`   | Add the eryph guest-services geneset as fodder and let **cloud-init** install it (natural Linux / AWS path). |
| `none`      | The image already ships eryph guest services; nothing to inject. |

The value is a strategy, **not** a concrete extension name or geneset reference,
so it can evolve without editing every base catlet. The enum is open for future
values.

### Reference types

#### `azure_marketplace` (cloud: `azure`)

| Field     | Required | Notes |
|-----------|----------|-------|
| `publisher` | yes | Marketplace publisher. |
| `offer`     | yes | Marketplace offer. |
| `sku`       | yes | SKU. Generation/security is encoded here (e.g. `-g2` ⇒ Gen2 / Trusted Launch). |
| `version`   | no  | `latest` (default, rolls forward) or a pinned version. |
| `plan`      | no  | `{ publisher, product, name }` for paid / BYOL images. |

#### `ec2_ssm_parameter` (cloud: `ec2`)

| Field       | Required | Notes |
|-------------|----------|-------|
| `parameter` | yes | Public SSM parameter path resolving to an AMI id (e.g. `/aws/service/ami-windows-latest/...`). Region-independent, rolls forward. |

#### `ec2_image_filter` (cloud: `ec2`)

For images with no published SSM parameter. Selects the most recent matching AMI.

| Field          | Required | Notes |
|----------------|----------|-------|
| `owner`        | yes | AMI owner alias or account id (e.g. `amazon`, Canonical `099720109477`). |
| `name_pattern` | yes | AMI name glob; newest match wins. |
| `architecture` | no  | e.g. `x86_64`, `arm64`. |

## Resolve flow

For a target cloud `C` and the catlet's primary drive:

1. Does the pinned tag (`GenesetTagManifestData.VolumeGenes`) contain a volume
   with `arch` = `C/<cpu>` (e.g. `azure/amd64`)? → **transform path**: use it.
   This always wins when present.
2. Otherwise read `geneset.json` `cloud_compatibility[drive]` and take the entries with
   `cloud == C`, in order.
3. For each candidate, validate it against the **catlet's** declared capabilities
   (e.g. `secure_boot` + `tpm` require an Azure Gen2/Trusted-Launch SKU /
   AWS UEFI + NitroTPM image). Reject mismatches rather than discovering them at
   boot.
4. Resolve the first valid candidate to a concrete image (roll forward) and
   apply its `guest_services_injection` strategy.
5. If neither a transformed volume nor a usable mapping exists for `C`, either
   fall back to transform-and-upload on demand or fail with a clear message.

## Suggested model changes (`Eryph.GenePool.Model`)

Add to `GenesetManifestData`:

```csharp
[JsonPropertyName("cloud_compatibility")]
public Dictionary<string, CloudImageReference[]>? CloudCompatibility { get; set; }
```

New types (sketch — `cloud` + `type` are the discriminators; a polymorphic
`JsonConverter` keyed on `type` is the natural way to bind the bodies):

```csharp
public class CloudImageReference
{
    [JsonPropertyName("cloud")]                    // azure | ec2  (Hypervisors.KnownNames)
    public string? Cloud { get; set; }

    [JsonPropertyName("type")]                     // azure_marketplace | ec2_ssm_parameter | ec2_image_filter
    public string? Type { get; set; }

    [JsonPropertyName("guest_services_injection")] // extension | geneset | none
    public string? GuestServicesInjection { get; set; }

    // azure_marketplace
    [JsonPropertyName("publisher")] public string? Publisher { get; set; }
    [JsonPropertyName("offer")]     public string? Offer { get; set; }
    [JsonPropertyName("sku")]       public string? Sku { get; set; }
    [JsonPropertyName("version")]   public string? Version { get; set; }
    [JsonPropertyName("plan")]      public AzurePlan? Plan { get; set; }

    // ec2_ssm_parameter
    [JsonPropertyName("parameter")] public string? Parameter { get; set; }

    // ec2_image_filter
    [JsonPropertyName("owner")]        public string? Owner { get; set; }
    [JsonPropertyName("name_pattern")] public string? NamePattern { get; set; }
    [JsonPropertyName("architecture")] public string? Architecture { get; set; }
}
```

Consider new constant holders to mirror existing ones (`Hypervisors`,
`Architectures`): `CloudImageReferenceTypes` (`azure_marketplace`, …) and
`GuestServicesInjectionMethods` (`extension`, `geneset`, `none`), plus validation in
`ManifestValidations`.

## Formal JSON Schema (draft 2020-12)

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://eryph.io/schemas/geneset-compatibility.json",
  "title": "Geneset cloud image compatibility",
  "type": "object",
  "properties": {
    "cloud_compatibility": {
      "type": "object",
      "description": "Map of drive name to ordered list of cloud image candidates.",
      "additionalProperties": {
        "type": "array",
        "items": { "$ref": "#/$defs/cloudImageReference" }
      }
    }
  },
  "$defs": {
    "guestServicesInjection": {
      "type": "string",
      "enum": ["extension", "geneset", "none"]
    },
    "cloudImageReference": {
      "type": "object",
      "required": ["cloud", "type"],
      "properties": {
        "cloud": { "type": "string", "enum": ["azure", "ec2"] },
        "type": { "type": "string" },
        "guest_services_injection": { "$ref": "#/$defs/guestServicesInjection" }
      },
      "oneOf": [
        {
          "title": "azure_marketplace",
          "properties": {
            "cloud": { "const": "azure" },
            "type": { "const": "azure_marketplace" },
            "publisher": { "type": "string" },
            "offer": { "type": "string" },
            "sku": { "type": "string" },
            "version": { "type": "string", "default": "latest" },
            "plan": {
              "type": "object",
              "properties": {
                "publisher": { "type": "string" },
                "product": { "type": "string" },
                "name": { "type": "string" }
              },
              "required": ["publisher", "product", "name"]
            },
            "guest_services_injection": { "$ref": "#/$defs/guestServicesInjection" }
          },
          "required": ["publisher", "offer", "sku"],
          "additionalProperties": false
        },
        {
          "title": "ec2_ssm_parameter",
          "properties": {
            "cloud": { "const": "ec2" },
            "type": { "const": "ec2_ssm_parameter" },
            "parameter": { "type": "string" },
            "guest_services_injection": { "$ref": "#/$defs/guestServicesInjection" }
          },
          "required": ["parameter"],
          "additionalProperties": false
        },
        {
          "title": "ec2_image_filter",
          "properties": {
            "cloud": { "const": "ec2" },
            "type": { "const": "ec2_image_filter" },
            "owner": { "type": "string" },
            "name_pattern": { "type": "string" },
            "architecture": { "type": "string" },
            "guest_services_injection": { "$ref": "#/$defs/guestServicesInjection" }
          },
          "required": ["owner", "name_pattern"],
          "additionalProperties": false
        }
      ]
    }
  }
}
```

## Open points for implementation

- **Capability ↔ generation validation** lives in the resolve engine (eryph-zero),
  not in the genepool client; the client only models and validates the manifest
  shape.
- **Cloud naming**: this doc uses `ec2` for AWS to match `Hypervisors`/
  `Architectures`. Confirm `type` prefixes (`ec2_*`) follow the same convention.
- **Explicit version pinning** (opt-in, for reproducibility) is intentionally
  out of scope here; the field set leaves room (`version` on Azure; a pinned AMI
  id type for EC2) when needed.
```
