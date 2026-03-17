import { Card, List, Typography } from "antd";
import { useFlightSearchStore } from "../store/flightSearchStore";
import type { FlightOptionResponse } from "../services/flightSearchService";

const { Text } = Typography;

const formatDateTime = (value: string) => {
    const d = new Date(value);
    return d.toLocaleString(undefined, { dateStyle: "short", timeStyle: "short" });
};

const formatPrice = (value: number) =>
    new Intl.NumberFormat(undefined, { style: "currency", currency: "USD" }).format(value);

const FlightOptionItem = ({ option }: { option: FlightOptionResponse }) => (
    <List.Item>
        <div style={{ width: "100%" }}>
            <div style={{ display: "flex", justifyContent: "space-between" }}>
                <Text strong>{option.flightNumber}</Text>
                <Text>{option.airline}</Text>
            </div>
            <div style={{ display: "flex", justifyContent: "space-between", marginTop: 4 }}>
                <Text type="secondary">{option.origin} → {option.destination}</Text>
            </div>
            <div style={{ marginTop: 2 }}>
                <Text type="secondary" style={{ fontSize: 12 }}>
                    {formatDateTime(option.departureTime)} – {formatDateTime(option.arrivalTime)}
                </Text>
            </div>
            <div style={{ display: "flex", justifyContent: "space-between", marginTop: 4 }}>
                <Text>{formatPrice(option.pricePerPerson)} / person</Text>
                <Text type="secondary">{option.availableSeats} seats</Text>
            </div>
        </div>
    </List.Item>
);

const FlightSearch = () => {
    const flightSearch = useFlightSearchStore((s) => s.flightSearch);
    if (!flightSearch) return null;

    return (
        <Card title="Flight Options" size="small" style={{ border: "1px solid #d9d9d9" }}>
            <List
                size="small"
                dataSource={flightSearch.flightOptions}
                renderItem={(option) => <FlightOptionItem key={option.id} option={option} />}
            />
        </Card>
    );
};

export default FlightSearch;
