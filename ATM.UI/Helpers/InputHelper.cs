using Spectre.Console;

namespace ATM.UI.Helpers;

public class InputHelper
{
  public static decimal AskPositiveAmount(string message)
  {
    return AnsiConsole.Prompt(
      new TextPrompt<decimal>(message)
        .Validate(amount =>
        {
          if (amount <= 0)
          {
            return ValidationResult.Error(
              "[red]Amount must be greater than zero.[/]"
            );
          }

          return ValidationResult.Success();
        })
    );
  }
}
