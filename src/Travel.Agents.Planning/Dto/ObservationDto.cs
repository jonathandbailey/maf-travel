namespace Travel.Agents.Planning.Dto;

public record ObservationDto(string Source, string Status, string Summary, Dictionary<string, object?> MetaData);