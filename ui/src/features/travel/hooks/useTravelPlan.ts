import { useEffect } from "react";
import { EventType, type BaseEvent, type StateSnapshotEvent } from "@ag-ui/client";
import { useTravelPlanStore } from "../store/travelPlanStore";
import type { ChatAgentClient } from "../../chat/services/ChatAgentClient";

interface TravelPlanUpdatePayload {
    Origin: string | null;
    Destination: string | null;
    StartDate: string | null;
    EndDate: string | null;
    NumberOfTravelers: number | null;
}

interface TravelPlanSnapshot {
    Type: string;
    Payload?: TravelPlanUpdatePayload;
}

export function useTravelPlan(client: ChatAgentClient) {
    const { updatePlan } = useTravelPlanStore();

    useEffect(() => {
        const handleEvent = (event: BaseEvent) => {
            if (event.type !== EventType.STATE_SNAPSHOT) return;

            const snapshot = (event as StateSnapshotEvent).snapshot as TravelPlanSnapshot;
            if (typeof snapshot !== 'object' || snapshot === null || !('Type' in snapshot)) return;
            if (snapshot.Type !== 'TravelPlanUpdate') return;

            const payload = snapshot.Payload;
            if (!payload) return;

            updatePlan({
                origin: payload.Origin ?? null,
                destination: payload.Destination ?? null,
                startDate: payload.StartDate ?? null,
                endDate: payload.EndDate ?? null,
                numberOfTravelers: payload.NumberOfTravelers ?? null,
            });
        };

        client.addEventHandler(handleEvent);
        return () => client.removeEventHandler(handleEvent);
    }, [client, updatePlan]);

}
