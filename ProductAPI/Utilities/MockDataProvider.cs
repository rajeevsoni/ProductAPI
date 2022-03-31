using ProductAPI.Models;

namespace ProductAPI.Utilities
{
    public static class MockDataProvider
    {
        public static IList<ColourResponse> GetColourInfos()
        {
            IList<ColourResponse> colourInfos = new List<ColourResponse>();
            ColourResponse colourResponse = new ColourResponse();
            colourResponse.ColourId = Guid.Parse("fb8ce6b2-fa0a-4bfd-89ed-81affa0fe859");
            colourResponse.ColourName = "Black";
            colourResponse.ColourCode = "#000000";
            colourInfos.Add(colourResponse);
            return colourInfos;
        }

        public static  IList<SizeResponse> GetSizeInfos()
        {
            IList<SizeResponse> sizeInfos = new List<SizeResponse>();
            SizeResponse sizeResponse = new SizeResponse();
            sizeResponse.SizeId = Guid.Parse("53697b70-8059-4324-a33f-f56e1b1209eb");
            sizeResponse.SizeName = "Medium";
            sizeInfos.Add(sizeResponse);
            return sizeInfos; ;
        }
    }
}
