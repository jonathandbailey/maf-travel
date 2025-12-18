
import { Card, Flex, Typography } from "antd";
import type { FlightOptionDto } from "../../api/travel.dto";
import dayjs from 'dayjs';
import advancedFormat from 'dayjs/plugin/advancedFormat';

const { Text } = Typography;

dayjs.extend(advancedFormat);

const formatDate = (dateString: string | undefined): string => {
    if (!dateString) return '';
    return dayjs(dateString).format('Do, MMM, YYYY - h:mm A');
};

interface FlightProps {
    flight: FlightOptionDto;
}

const Flight = ({ flight }: FlightProps) => {
    return (
        <Card
            size="small"

            style={{ boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)", width: "300px" }}
        >

            <div>
                <Flex gap="small" align="center">
                    <Text strong style={{ fontSize: '20px' }}>{flight.airline}</Text>
                    <Text type="secondary" style={{ fontSize: '12px' }}>{flight.flightNumber}</Text>
                </Flex>
                <Flex vertical gap="extra-small">
                    <Text type="secondary" style={{ fontSize: '12px' }}>Depart</Text>
                    <Text style={{ fontSize: '14px' }}>{flight.departure.airport}</Text>
                    <Text style={{ fontSize: '14px' }}>{formatDate(flight.departure.datetime)}</Text>
                </Flex>
                <Flex vertical gap="extra-small">
                    <Text type="secondary" style={{ fontSize: '12px' }}>Arrive</Text>
                    <Text style={{ fontSize: '14px' }}>{flight.arrival.airport}</Text>
                    <Text style={{ fontSize: '14px' }}>{formatDate(flight.arrival.datetime)}</Text>
                </Flex>

                <p>{flight.duration} | {flight.price.amount} {flight.price.currency}</p>

            </div>
        </Card>

    );
}

export default Flight;