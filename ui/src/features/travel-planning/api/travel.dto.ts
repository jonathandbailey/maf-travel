export interface TravelPlanDto {
    destination: string;
    startDate: string;
    endDate: string;
    origin: string;
    flightPlan: FlightPlanDto;
}

export interface FlightSearchResultDto {
    artifactKey: string;
    results: FlightOptionDto[];
}

export interface FlightOptionDto {
    airline: string;
    flightNumber: string;
    departure: FlightEndpointDto;
    arrival: FlightEndpointDto;
    duration: string;
    price: PriceDto;
}

export interface FlightEndpointDto {
    airport: string;
    datetime: string;
}

export interface PriceDto {
    amount: number;
    currency: string;
}


export interface FlightPlanDto {
    flightOption?: FlightOptionDto | null;
}

export interface HotelSearchResultDto {
    artifactKey: string;
    results: HotelOptionDto[];
}

export interface HotelOptionDto {
    hotelName: string;
    address: string;
    checkIn: string;
    checkOut: string;
    rating: number;
    pricePerNight: HotelPriceDto;
    totalPrice: HotelPriceDto;
}

export interface HotelPriceDto {
    amount: number;
    currency: string;
}