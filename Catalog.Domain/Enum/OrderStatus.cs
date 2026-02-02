namespace Catalog.Contracts.Enum
{
    public enum OrderStatus
    {
        /// <summary> Черновик (сохранен, но не оформлен) </summary>
        Draft = 1,

        /// <summary> На согласовании </summary>
        PendingApproval = 2,

        /// <summary> Согласован </summary>
        ApprovalCompleted = 3,

        /// <summary> Передан в производство </summary>
        InProduction = 4,

        /// <summary> Не принят в производство</summary>
        RejectedFromProduction = 5,

        /// <summary> Производство завершено </summary>
        Produced = 6,

        /// <summary> Отправлен клиенту </summary>
        Shipped = 7,

        /// <summary> Доставлен </summary>
        Delivered = 8,

        /// <summary> Выполнен/Завершен </summary>
        Completed = 9,

        /// <summary> Отменен </summary>
        Cancelled = 10
    }

    public static class OrderStatusExtensions
    {
        public static string ToRussianString(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Draft => "Черновик",
                //OrderStatus.Placed => "Оформлен",
                OrderStatus.PendingApproval => "На согласовании",
                OrderStatus.ApprovalCompleted => "Согласован",
                OrderStatus.RejectedFromProduction => "Не принят в производство",
                OrderStatus.InProduction => "Передан в производство",
                OrderStatus.Produced => "Производство завершено",
                OrderStatus.Shipped => "Отправлен клиенту",
                OrderStatus.Completed => "Завершен",
                OrderStatus.Cancelled => "Отменен",
                _ => status.ToString()
            };
        }


    }
}
