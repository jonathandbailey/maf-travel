import { useEffect } from "react";
import { TravelService } from "../api/travel.api";
import streamingService from "../../../app/api/streaming.api";
import type { TravelPlanDto } from "../api/travel.dto";

interface UseExchangeStatusUpdateHandlerProps {
    sessionId: string;
    setTravelPlan: React.Dispatch<React.SetStateAction<TravelPlanDto | null>>;
}

export const useTravelPlanUpdateHandler = ({ sessionId, setTravelPlan }: UseExchangeStatusUpdateHandlerProps) => {
    useEffect(() => {

        const handleExchangeStatusUpdate = () => {

            const travelService = new TravelService();
            travelService.getTravelPlan(sessionId)

                .then((travelPlan: TravelPlanDto) => {
                    setTravelPlan(travelPlan);
                })

        };

        streamingService.on("travelPlan", handleExchangeStatusUpdate);

        return () => {

            streamingService.off("travelPlan", handleExchangeStatusUpdate);
        };
    }, [setTravelPlan]);
};