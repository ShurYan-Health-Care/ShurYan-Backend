namespace Shuryan.Application.DTOs.Responses.Laboratory
{

        public class LaboratoryHomeSampleCollectionResponse
        {
                /// هل المعمل يقدم خدمة سحب العينة من البيت؟
                public bool OffersHomeSampleCollection { get; set; }

                /// رسوم خدمة سحب العينة من البيت
                public decimal? HomeSampleCollectionFee { get; set; }
        }
}
