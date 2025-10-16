using Bancalite.Application.Core;
using Bancalite.Persitence.Model;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Bancalite.Application.Auth.ResetPassword;

/// <summary>
/// Comando para aplicar el token y nueva contraseña.
/// </summary>
public class ResetPasswordCommand
{
    public record ResetPasswordCommandRequest(ResetPasswordRequest Request) : IRequest<Result<bool>>;

    internal class Handler : IRequestHandler<ResetPasswordCommandRequest, Result<bool>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IValidator<ResetPasswordRequest> _validator;

        public Handler(UserManager<AppUser> userManager, IValidator<ResetPasswordRequest> validator)
        {
            _userManager = userManager;
            _validator = validator;
        }

        public async Task<Result<bool>> Handle(ResetPasswordCommandRequest message, CancellationToken ct)
        {
            await _validator.ValidateAndThrowAsync(message.Request, ct);

            var user = await _userManager.FindByEmailAsync(message.Request.Email);
            if (user is null)
            {
                return Result<bool>.Failure("Unauthorized");
            }

            var result = await _userManager.ResetPasswordAsync(user, message.Request.Token, message.Request.NewPassword);
            if (!result.Succeeded)
            {
                var error = string.Join("; ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure(error);
            }

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);
            return Result<bool>.Success(true);
        }
    }
}

