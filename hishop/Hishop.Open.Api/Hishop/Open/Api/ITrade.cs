namespace Hishop.Open.Api
{
    using System;

    public interface ITrade
    {
        string ChangLogistics(string tid, string company_name, string out_sid);
        string GetIncrementSoldTrades(DateTime start_modified, DateTime end_modified, string status, string buyer_uname, int page_no, int page_size);
        string GetSoldTrades(DateTime? start_created, DateTime? end_created, string status, string buyer_uname, int page_no, int page_size);
        string GetTrade(string tid);
        string SendLogistic(string tid, string company_name, string out_sid);
        string UpdateTradeMemo(string tid, string memo, int flag);
    }
}

