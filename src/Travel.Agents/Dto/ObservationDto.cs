namespace Travel.Agents.Dto;

public record ObservationDto(string Source, string Status, string Summary, Dictionary<string, object?> MetaData);