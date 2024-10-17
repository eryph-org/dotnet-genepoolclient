using System.CommandLine;
using LanguageExt;
using LanguageExt.Common;

namespace Eryph.Packer;

public static class CommandValidationExtensions
{
    public static Argument<T> AddValidation<T,TValidation>(
        this Argument<T> argument,
        Func<T, Validation<Error, TValidation>> validator)
    {

        argument.AddValidator(result =>
        {
            var value = result.GetValueOrDefault<T>();
            var validation = validator(value);
            var either = validation.ToEither().MapLeft(Error.Many).Map(_ => value);
            either.IfLeft(l => result.ErrorMessage = $"Invalid argument <{argument.Name}>:\n{ErrorHelper.PrintError(l)}");

        } );

        return argument;
    }

    public static System.CommandLine.Option<T> AddValidation<T, TValidation>(
        this System.CommandLine.Option<T> option,
        Func<T, Validation<Error, TValidation>> validator)
    {

        option.AddValidator(result =>
        {
            var value = result.GetValueOrDefault<T>();

            if (value == null || value.Equals(default(T)))
                return;

            var validation = validator(value);
            var either = validation.ToEither().MapLeft(Error.Many).Map(_ => value);
            either.IfLeft(l => result.ErrorMessage = $"Invalid option {option.Name}:\n{ErrorHelper.PrintError(l)}");

        });

        return option;
    }
}