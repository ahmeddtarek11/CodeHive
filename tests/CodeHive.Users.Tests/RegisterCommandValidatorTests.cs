using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHive.Users.Application.Commands.Register;
using FluentValidation.Results;
using Xunit;

namespace CodeHive.Users.Tests;

public class RegisterCommandValidatorTests
{
  private readonly RegisterCommandValidator _validator;

  public RegisterCommandValidatorTests()
  {
    _validator =  new RegisterCommandValidator();
  }


    [Fact]
    public void ValidateWhenCommandIsValid_ReturnsNoErros()
    {
        // ARRANGE
        var command = new RegisterCommand("valid@test.com" , "valid_user" , "StrongPassword123!" , "ValidDisplayName");
    
        // ACT 
        ValidationResult result = _validator.Validate(command);
    
        // Assert
         Assert.True(result.IsValid);
         Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("")]
    public void ValidateWhenEmailInvlaid_ReturnsError(string invalidEmail)
    {
        // ARRANGE
        var command = new RegisterCommand(invalidEmail, "valid_user", "StrongPass123!", "Valid Display");

            //ACT
          var result = _validator.Validate(command);


          //ASSERT
          Assert.False(result.IsValid);
          Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }





}
