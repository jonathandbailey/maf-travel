using Travel.Agents.Dto;

namespace Travel.Tests.Helper;

public static  class TestHelper
{
   

    public static RequestInformationDto CreateInformationRequest()
    {
        var informationRequest =
            new RequestInformationDto(
                "Travel Plan Information is missing.","End Date is requird to to complete the travel planning.",
                [ "EndDate"]);

        return informationRequest;
    }
}