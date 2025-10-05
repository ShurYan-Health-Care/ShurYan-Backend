using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuryan.Core.Enums.Notifications
{
	public enum NotificationType
	{
		// Appointment Notifications
		[Description("تأكيد الحجز")]
		AppointmentConfirmed = 1,

		[Description("تذكير بالموعد")]
		AppointmentReminder = 2,

		[Description("إلغاء الموعد")]
		AppointmentCancelled = 3,

		[Description("تعديل الموعد")]
		AppointmentRescheduled = 4,

		[Description("الدكتور جاهز")]
		AppointmentDoctorReady = 5,

		[Description("الموعد انتهى")]
		AppointmentCompleted = 6,

		// Prescription Notifications
		[Description("روشته طبية جديدة")]
		PrescriptionCreated = 7,

		[Description("الروشته جاهزة للطلب")]
		PrescriptionReadyToOrder = 8,

		[Description("تذكير بالدواء")]
		MedicationReminder = 9,

		[Description("انتهاء الروشته")]
		PrescriptionExpired = 10,

		// Lab Order Notifications
		[Description("طلب تحاليل جديد")]
		LabOrderCreated = 11,

		[Description("جاهز للدفع")]
		LabOrderReadyToPay = 12,

		[Description("تم التأكيد")]
		LabOrderConfirmed = 13,

		[Description("موظف المعمل في الطريق")]
		LabOrderSampleCollectionOnWay = 14,

		[Description("النتائج جاهزة")]
		LabOrderResultsReady = 15,

		[Description("تقييم التجربة")]
		LabOrderRequestReview = 16,

		// Pharmacy Order Notifications
		[Description("طلب صيدلية جديد")]
		PharmacyOrderCreated = 17,

		[Description("قيد التحضير")]
		PharmacyOrderInPreparation = 18,

		[Description("خرج للتوصيل")]
		PharmacyOrderOutForDelivery = 19,

		[Description("تم التسليم")]
		PharmacyOrderDelivered = 20,

		[Description("تم الإلغاء")]
		PharmacyOrderCancelled = 21,

		// Verification Notifications
		[Description("المستندات قيد المراجعة")]
		VerificationUnderReview = 22,

		[Description("تم التحقق")]
		VerificationApproved = 23,

		[Description("تم الرفض")]
		VerificationRejected = 24,

		[Description("الحساب معلق")]
		AccountSuspended = 25,

		// Review Notifications
		[Description("تقييم جديد")]
		NewReview = 26,

		[Description("تعليق جديد")]
		NewComment = 27,

		// Security Notifications
		[Description("تسجيل دخول جديد")]
		NewLogin = 28,

		[Description("تغيير كلمة المرور")]
		PasswordChanged = 29,

		[Description("تغيير البريد الإلكتروني")]
		EmailChanged = 30,

		// General Notifications
		[Description("عرض خاص")]
		SpecialOffer = 31,

		[Description("إشعار عام")]
		GeneralNotification = 32
	}
}
