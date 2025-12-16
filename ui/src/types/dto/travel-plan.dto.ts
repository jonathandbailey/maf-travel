import type { FlightOptionDto } from "./flight.dto";

export interface TravelPlanDto {
    destination: string;
    startDate: string;
    endDate: string;
    origin: string;
    selectedFlightOption?: FlightOptionDto | null;
}