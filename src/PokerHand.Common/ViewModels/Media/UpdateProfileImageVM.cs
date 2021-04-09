using System;
using FluentValidation;

namespace PokerHand.Common.ViewModels.Media
{
    public class UpdateProfileImageVM
    {
        public Guid PlayerId { get; set; }
        public string NewProfileImage { get; set; }
    }

    public class UpdateProfileImageValidator : AbstractValidator<UpdateProfileImageVM>
    {
        public UpdateProfileImageValidator()
        {
            RuleFor(x => x.PlayerId).NotEmpty().WithMessage("PlayerId should not be empty");
            RuleFor(x => x.NewProfileImage).NotEmpty().WithMessage("Image should not be empty");
        }
    }
}