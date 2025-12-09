import { Card, Rate } from "antd";
import type { HotelOptionDto } from "../../../types/dto/hotel.dto";

interface HotelProps {
    hotel: HotelOptionDto;
}

const Hotel = ({ hotel }: HotelProps) => {
    return (
        <>
            <Card size="small" title={hotel.hotelName}>
                <p>{hotel.address}</p>
                <Rate disabled defaultValue={hotel.rating} />

                <p>Price per night: {hotel.pricePerNight.amount} {hotel.pricePerNight.currency}</p>
                <p>Total price: {hotel.totalPrice.amount} {hotel.totalPrice.currency}</p>
            </Card>
            <div>

            </div>
        </>

    );
}

export default Hotel;