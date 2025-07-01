
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TMS_MAIN.Data;
using TMS_MAIN.Models;
using TMS_MAIN.ViewModels;

namespace TMS_MAIN.Services
{
    public class InvestmentService : IInvestmentService
    {
        private readonly TreasuryManagementSystemContext _context;

        public InvestmentService(TreasuryManagementSystemContext context)
        {
            _context = context;
        }

        public IEnumerable<Investment> GetInvestments()
        {
            return _context.Investments
                           .Include(i => i.User)
                           .OrderByDescending(i => i.PurchaseDate) // or another relevant date field
                           .ToList();
        }

        public Investment GetInvestmentById(int id)
        {
            return _context.Investments.FirstOrDefault(c => c.InvestmentId == id);
        }

        public void AddInvestment(Investment investment)
        {
            _context.Investments.Add(investment);
            _context.SaveChanges();
        }

        public void UpdateInvestment(Investment updatedInvestment)
        {
            var existingInvestment = _context.Investments.FirstOrDefault(i => i.InvestmentId == updatedInvestment.InvestmentId);
            if (existingInvestment != null)
            {
                existingInvestment.InvestmentType = updatedInvestment.InvestmentType;
                existingInvestment.AmountInvested = updatedInvestment.AmountInvested;
                existingInvestment.CurrentValue = updatedInvestment.CurrentValue;
                existingInvestment.PurchaseDate = updatedInvestment.PurchaseDate;
                existingInvestment.MaturityDate = updatedInvestment.MaturityDate;

                _context.SaveChanges();
            }
        }

        public void DeleteInvestment(int id)
        {
            var investment = _context.Investments.Find(id);
            if (investment != null)
            {
                _context.Investments.Remove(investment);
                _context.SaveChanges();
            }
        }

        public bool InvestmentExists(int id)
        {
            return _context.Investments.Any(i => i.InvestmentId == id);
        }

        public IEnumerable<Investment> GetInvestmentsByUserId(int userId)
        {
            return _context.Investments.Where(i => i.UserId == userId).ToList();
        }

        /*public PortfolioSummaryViewModel GetPortfolioSummary()
        {
            // Replace with actual logic to calculate these values from your database
            return new PortfolioSummaryViewModel
            {
                TotalInvested = _context.Investments.Sum(i => i.AmountInvested),
                TotalCurrentValue = _context.Investments.Sum(i => i.CurrentValue),
                NumberOfInvestments = _context.Investments.Count(),
                HighestInvestmentValue = _context.Investments.Max(i => i.CurrentValue),
                LowestInvestmentValue = _context.Investments.Min(i => i.CurrentValue),
                LastUpdated = DateTime.Now,
                InvestmentBreakdown = _context.Investments.Select(i => new InvestmentBreakdownViewModel
                {
                    InvestmentType = i.InvestmentType.ToString(),
                    AmountInvested = i.AmountInvested,
                    CurrentValue = i.CurrentValue
                }).ToList()
            };
        }*/
        public PortfolioSummaryViewModel GetPortfolioSummary(int userId)
        {
            var userInvestments = _context.Investments.Where(i => i.UserId == userId).ToList();

            return new PortfolioSummaryViewModel
            {
                TotalInvested = userInvestments.Sum(i => i.AmountInvested),
                TotalCurrentValue = userInvestments.Sum(i => i.CurrentValue),
                NumberOfInvestments = userInvestments.Count,
                HighestInvestmentValue = userInvestments.Any() ? userInvestments.Max(i => i.CurrentValue) : 0,
                LowestInvestmentValue = userInvestments.Any() ? userInvestments.Min(i => i.CurrentValue) : 0,
                LastUpdated = DateTime.Now,
                InvestmentBreakdown = userInvestments.Select(i => new InvestmentBreakdownViewModel
                {
                    InvestmentType = i.InvestmentType.ToString(),
                    AmountInvested = i.AmountInvested,
                    CurrentValue = i.CurrentValue
                }).ToList()
            };
        }

    }
}
