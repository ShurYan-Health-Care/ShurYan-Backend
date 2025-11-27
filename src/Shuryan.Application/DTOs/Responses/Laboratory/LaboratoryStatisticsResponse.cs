namespace Shuryan.Application.DTOs.Responses.Laboratory
{
        /// <summary>
        /// إحصائيات تفصيلية للمعمل
        /// </summary>
        public class LaboratoryStatisticsResponse
        {
                public DateTime StartDate { get; set; }
                public DateTime EndDate { get; set; }

                public int TotalOrders { get; set; }
                public int CompletedOrders { get; set; }
                public int CancelledOrders { get; set; }
                public decimal TotalRevenue { get; set; }
                public decimal AverageOrderValue { get; set; }
                public double CompletionRate { get; set; }

                public List<DailyStatistics> DailyStatistics { get; set; } = new();
                public List<StatusBreakdown> StatusBreakdown { get; set; } = new();
        }

        public class DailyStatistics
        {
                public DateTime Date { get; set; }
                public int OrdersCount { get; set; }
                public int CompletedCount { get; set; }
                public decimal Revenue { get; set; }
        }

        public class StatusBreakdown
        {
                public string Status { get; set; } = string.Empty;
                public int Count { get; set; }
                public double Percentage { get; set; }
        }
}
