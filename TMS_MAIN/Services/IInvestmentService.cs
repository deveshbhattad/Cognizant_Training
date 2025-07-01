
using System.Collections.Generic;
using TMS_MAIN.Models;
using TMS_MAIN.ViewModels;

namespace TMS_MAIN.Services
{
    public interface IInvestmentService
    {
        // List<Investment> GetInvestments();//For Admin
        //PortfolioSummaryViewModel GetPortfolioSummary();
        PortfolioSummaryViewModel GetPortfolioSummary(int userId);
        IEnumerable<Investment> GetInvestments();
        void AddInvestment(Investment investment);
        Investment GetInvestmentById(int id);
        void UpdateInvestment(Investment investment);
        void DeleteInvestment(int id);
        bool InvestmentExists(int id);
        IEnumerable<Investment> GetInvestmentsByUserId(int userId);
    }
}
