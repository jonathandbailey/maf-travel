import { Flex } from "antd";
import type { HotelOptionDto } from "../../../types/dto/hotel.dto";
import Hotel from "./Hotel";

interface HotelListProps {
    hotels: HotelOptionDto[];
}

const HotelList = ({ hotels }: HotelListProps) => {
    return (
        <div>
            <Flex gap="middle" wrap="wrap" justify="center">
                {hotels.map((hotel, index) => (
                    <Hotel key={index} hotel={hotel} />
                ))}
            </Flex>

        </div>
    );
}

export default HotelList;