import { Card, Flex, Rate, Typography } from "antd";
import type { HotelOptionDto } from "../../../types/dto/hotel.dto";

interface HotelProps {
    hotel: HotelOptionDto;
}

const { Text } = Typography;

const Hotel = ({ hotel }: HotelProps) => {
    return (
        <>
            <Card
                size="small"

                style={{ boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)", width: "300px" }}
            >
                <Flex vertical gap="extra-small">

                    <Text strong style={{ fontSize: '20px' }}>{hotel.hotelName}</Text>
                    <Text type="secondary" style={{ fontSize: '12px' }}>{hotel.address}</Text>

                    <Rate disabled defaultValue={hotel.rating} />

                    <p>Price per night: {hotel.pricePerNight.amount} {hotel.pricePerNight.currency}</p>
                    <p>Total price: {hotel.totalPrice.amount} {hotel.totalPrice.currency}</p>
                </Flex>

            </Card>
            <div>

            </div>
        </>

    );
}

export default Hotel;