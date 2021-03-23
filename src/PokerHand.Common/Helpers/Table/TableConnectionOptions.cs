using System;
using FluentValidation;

namespace PokerHand.Common.Helpers.Table
{
    public class TableConnectionOptions
    {
        public TableTitle TableTitle { get; set; }
        public Guid PlayerId { get; set; }
        public Guid? CurrentTableId { get; set; }
        public string PlayerConnectionId { get; set; }
        public int BuyInAmount { get; set; }
        public bool IsAutoTop { get; set; }
    }
    
    public class TableConnectionOptionsValidator: AbstractValidator<TableConnectionOptions>
    {
        public TableConnectionOptionsValidator()
        {
            RuleFor(m => m.TableTitle).NotEmpty().WithMessage("Table title  is required");
            RuleFor(m => m.PlayerId).NotEmpty().WithMessage("Player Id is required");
        }
    }
}