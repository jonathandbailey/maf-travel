import { useEffect } from "react";
import { EventType, type BaseEvent, type StateSnapshotEvent } from "@ag-ui/client";
import { useFlightSearchStore } from "../store/flightSearchStore";
import { getFlightSearch } from "../services/flightSearchService";
import type { ChatAgentClient } from "../../chat/services/ChatAgentClient";

interface ArtifactCreatedPayload {
    ArtifactType: string;
    Id: string;
}

interface ArtifactCreatedSnapshot {
    Type: string;
    Payload?: ArtifactCreatedPayload;
}

export function useFlightSearch(client: ChatAgentClient) {
    const { addFlightSearch } = useFlightSearchStore();

    useEffect(() => {
        const handleEvent = (event: BaseEvent) => {
            if (event.type !== EventType.STATE_SNAPSHOT) return;

            const snapshot = (event as StateSnapshotEvent).snapshot as ArtifactCreatedSnapshot;
            if (typeof snapshot !== 'object' || snapshot === null || !('Type' in snapshot)) return;
            if (snapshot.Type !== 'ArtifactCreated') return;

            const payload = snapshot.Payload;
            if (!payload || payload.ArtifactType !== 'FlightSearch') return;

            console.log(`Fetching flight search results for artifact ID: ${payload.Id}`);

            getFlightSearch(payload.Id).then(addFlightSearch).catch(console.error);
        };

        client.addEventHandler(handleEvent);
        return () => client.removeEventHandler(handleEvent);
    }, [client, addFlightSearch]);
}
