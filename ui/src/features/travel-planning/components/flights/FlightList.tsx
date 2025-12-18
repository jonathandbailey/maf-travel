import { Flex } from "antd";
import type { FlightOptionDto } from "../../../types/dto/flight.dto";
import Flight from "./Flight";


interface FlightListProps {
    flights: FlightOptionDto[];
}

const FlightList = ({ flights }: FlightListProps) => {
    return (
        <Flex gap="middle" wrap="wrap" justify="center">
            {flights.map((flight, index) => (
                <Flight key={index} flight={flight} />
            ))}

        </Flex>


    );
}

export default FlightList;