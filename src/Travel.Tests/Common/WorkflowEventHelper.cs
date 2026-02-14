using FluentAssertions;
using Microsoft.Agents.AI.Workflows;
using Travel.Agents.Dto;
using Travel.Workflows.Dto;
using Travel.Workflows.Events;

namespace Travel.Tests.Common;

public static class WorkflowEventHelper
{
    public static WorkflowEventCollectionAssertions ShouldHaveEvent(this IEnumerable<WorkflowEvent> events)
    {
        return new WorkflowEventCollectionAssertions(events);
    }

    public static CheckpointInfo GetCheckpointInfo(this List<WorkflowEvent> events)
    {
        var superStepEvents = events.OfType<SuperStepCompletedEvent>().ToList();

        if (superStepEvents.Count == 0)
        {
            throw new InvalidOperationException("No SuperStepCompletedEvent found in the workflow events.");
        }

        var latestSuperStep = superStepEvents
            .OrderByDescending(e => e.StepNumber)
            .First();

        var checkpointInfo = latestSuperStep.CompletionInfo?.Checkpoint;

        if (checkpointInfo == null)
        {
            throw new InvalidOperationException("No checkpoint information found in the latest SuperStepCompletedEvent.");
        }

        return checkpointInfo;
    }
}

public class WorkflowEventCollectionAssertions
{
    private readonly IEnumerable<WorkflowEvent> _subject;

    public WorkflowEventCollectionAssertions(IEnumerable<WorkflowEvent> events)
    {
        _subject = events;
    }

    public AndConstraint<WorkflowEventCollectionAssertions> HaveCount(int expected, string because = "", params object[] becauseArgs)
    {
        var actualCount = _subject.Count();

        actualCount.Should().Be(expected, because, becauseArgs);

        return new AndConstraint<WorkflowEventCollectionAssertions>(this);
    }

    public AndConstraint<WorkflowEventCollectionAssertions> ShouldHaveType<T>(string because = "", params object[] becauseArgs) where T : WorkflowEvent
    {
        var matchingEvents = _subject.Where(e => e is T).ToList();

        var actualCount = matchingEvents.Count;
        actualCount.Should().BeGreaterThan(0, because, becauseArgs);

        return new AndConstraint<WorkflowEventCollectionAssertions>(this);
    }

    public AndConstraint<WorkflowEventCollectionAssertions> ShouldMatchFunctionCallResponse(RequestInformationDto informationRequestDetails, string because = "", params object[] becauseArgs)
    {
        var requestInfoEvent = _subject.OfType<RequestInfoEvent>().FirstOrDefault();

        requestInfoEvent.Should().NotBeNull($"collection should contain a {nameof(RequestInfoEvent)}");

        var data = requestInfoEvent!.Data as ExternalRequest;

        data.Should().NotBeNull();

        var request = data!.Data.AsType(typeof(InformationRequest)) as InformationRequest;

        request.Should().NotBeNull();

        request.Entities.Should().BeEquivalentTo(informationRequestDetails.RequiredInputs, because, becauseArgs);

        return new AndConstraint<WorkflowEventCollectionAssertions>(this);
    }

    public AndConstraint<WorkflowEventCollectionAssertions> ShouldMatchFunctionCallResponse(Travel.Workflows.Dto.TravelPlanDto travelPlanDto, string because = "", params object[] becauseArgs)
    {
        var travelPlanUpdateEvent = _subject.OfType<TravelPlanUpdateEvent>().FirstOrDefault();

        travelPlanUpdateEvent.Should().NotBeNull($"collection should contain a {nameof(TravelPlanUpdateEvent)}");

        var data = travelPlanUpdateEvent!.TravelPlanDto;

        data.Should().NotBeNull();

        data!.Origin.Should().Be(travelPlanDto.Origin, because, becauseArgs);
        data.Destination.Should().Be(travelPlanDto.Destination, because, becauseArgs);
        data.StartDate.Should().Be(travelPlanDto.StartDate, because, becauseArgs);
        data.EndDate.Should().Be(travelPlanDto.EndDate, because, becauseArgs);

        return new AndConstraint<WorkflowEventCollectionAssertions>(this);
    }
}
