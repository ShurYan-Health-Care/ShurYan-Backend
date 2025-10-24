namespace Shuryan.Core.Enums
{
    /// <summary>
    /// حالة الروشتة
    /// </summary>
    public enum PrescriptionStatus
    {
        /// <summary>
        /// نشطة - يمكن صرفها
        /// </summary>
        Active = 1,

        /// <summary>
        /// ملغاة - لا يمكن صرفها
        /// </summary>
        Cancelled = 2,

        /// <summary>
        /// تم صرفها
        /// </summary>
        Dispensed = 3,

        /// <summary>
        /// منتهية الصلاحية
        /// </summary>
        Expired = 4
    }
}
