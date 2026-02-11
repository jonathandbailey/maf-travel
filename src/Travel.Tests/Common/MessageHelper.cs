using System.Text.Json;
using Microsoft.Extensions.AI;
using Travel.Agents.Dto;

namespace Travel.Tests.Common;

public static class MessageHelper
{
    public class ObservationBuilder
    {
        private readonly Dictionary<string, object> _observation = new();

        public ObservationBuilder WithContext(string context)
        {
            _observation["context"] = context;
            return this;
        }

        public ObservationBuilder WithDestination(string destination)
        {
            _observation["destination"] = destination;
            return this;
        }

        public ObservationBuilder WithOrigin(string origin)
        {
            _observation["origin"] = origin;
            return this;
        }

        public ObservationBuilder WithStartDate(string startDate)
        {
            _observation["startDate"] = startDate;
            return this;
        }

        public ObservationBuilder WithEndDate(string endDate)
        {
            _observation["endDate"] = endDate;
            return this;
        }

        public ObservationBuilder WithNumberOfTravellers(int numberOfTravellers)
        {
            _observation["numberOfTravellers"] = numberOfTravellers;
            return this;
        }

        public Dictionary<string, object> Build()
        {
            return _observation;
        }
    }

    public static ObservationBuilder CreateObservation()
    {
        return new ObservationBuilder();
    }

    public static ChatMessage CreateObservationMessage(Dictionary<string, object> observation, TravelPlanDto? travelPlan = null)
    {
        var serializedObservation = JsonSerializer.Serialize(observation);
        var serializedPlan = JsonSerializer.Serialize(travelPlan ?? new TravelPlanDto());
        var template = $"Observation: {serializedObservation} \nTravelPlanSummary : {serializedPlan}";
        return new ChatMessage(ChatRole.User, template);
    }

    public static ChatMessage CreateTravelPlanMessage(TravelPlanDto travelPlan)
    {
        var serializedPlan = JsonSerializer.Serialize(travelPlan);
        var template = $"TravelPlanSummary : {serializedPlan}";
        return new ChatMessage(ChatRole.User, template);
    }
}