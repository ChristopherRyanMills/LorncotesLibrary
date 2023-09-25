using System.ComponentModel.DataAnnotations;

namespace LoncotesLibrary.Models;

public class Patron
{
    public int Id {get; set; }
    [Required]
    public string FirstName {get; set; }
    [Required]
    public string LastName {get; set; }
    [Required]
    public string Address {get; set; }
    [Required]
    public string Email {get; set; }
    public bool IsActive {get; set; }
    public List<Checkout> Checkouts {get; set; }
    public decimal? Balance {
        get
        {
            //filter checkouts to exclude any paid and not overdue
            //for each checkout remaining, add late fee
            List<Checkout> outstandingCheckouts = Checkouts.Where(c => c.Paid != true && c.LateFee != null).ToList();
            decimal? total = 0;
            foreach (Checkout checkout in outstandingCheckouts){
                total += checkout.LateFee;
            }
            return total;
        }}
}