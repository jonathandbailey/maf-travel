import { Flex } from "antd";
import Flight from "./Flight";
import type { FlightOptionDto } from "../../api/travel.dto";


interface FlightListProps {
    flights: FlightOptionDto[];
    returnFlights: FlightOptionDto[];
}

const FlightList = ({ flights, returnFlights }: FlightListProps) => {
    return (<>
        <Flex gap="middle" wrap="wrap" justify="center">
            {flights.map((flight, index) => (
                <Flight key={index} flight={flight} />
            ))}

        </Flex>
        <Flex gap="middle" wrap="wrap" justify="center">
            {returnFlights.map((flight, index) => (
                <Flight key={index} flight={flight} />
            ))}

        </Flex>
    </>



    );
}

export default FlightList;