import { Flex } from "antd";
import Hotel from "./Hotel";
import type { HotelOptionDto } from "../../api/travel.dto";

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