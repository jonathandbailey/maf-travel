import { useEffect, useRef } from "react";
import { EventType, type BaseEvent, type StateSnapshotEvent } from "@ag-ui/client";
import { useTravelPlanStore } from "../store/travelPlanStore";
import type { ChatAgentClient } from "../../chat/services/ChatAgentClient";

export function useTravelPlan(client: ChatAgentClient) {
    const { updatePlan, createPlan } = useTravelPlanStore();
    const clientRef = useRef(client);
    clientRef.current = client;

    useEffect(() => {
        const handleEvent = (event: BaseEvent, _exchangeId: string) => {
            if (event.type !== EventType.STATE_SNAPSHOT) return;

            const snapshot = (event as StateSnapshotEvent).snapshot;
            if (typeof snapshot !== 'object' || snapshot === null || !('Type' in snapshot)) return;
            if (snapshot.Type !== 'TravelPlanUpdate') return;

            const payload = (snapshot as any).Payload;
            if (!payload) return;

            updatePlan({
                origin: payload.Origin ?? null,
                destination: payload.Destination ?? null,
                startDate: payload.StartDate ?? null,
                endDate: payload.EndDate ?? null,
                numberOfTravelers: payload.NumberOfTravelers ?? null,
            });
        };

        clientRef.current.addEventHandler(handleEvent);
        return () => clientRef.current.removeEventHandler(handleEvent);
    }, [updatePlan]);

    const resetPlan = () => createPlan();

    return { resetPlan };
}
