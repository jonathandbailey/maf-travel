import { Card, Flex, Typography } from "antd";
import { ArrowRightOutlined } from "@ant-design/icons";
import dayjs from 'dayjs';
import advancedFormat from 'dayjs/plugin/advancedFormat';
import Flight from "../flights/Flight";
import type { TravelPlanDto } from "../../api/travel.dto";
import { useState } from "react";
import { useTravelPlanUpdateHandler } from "../../hooks/useTravelPlanUpdateHandler";

dayjs.extend(advancedFormat);

const { Text } = Typography;

interface TravelPlanProps {
    sessionId: string;
}

const formatDate = (dateString: string | undefined): string => {
    if (!dateString) return '';
    return dayjs(dateString).format('Do, MMM, YYYY');
};

const TravelPlan = ({ sessionId }: TravelPlanProps) => {

    const [travelPlan, setTravelPlan] = useState<TravelPlanDto | null>(null);

    const showOriginCard = !!(travelPlan?.origin || travelPlan?.startDate);
    const showDestinationCard = !!(travelPlan?.destination || travelPlan?.endDate);
    const showArrow = showOriginCard && showDestinationCard;



    useTravelPlanUpdateHandler({ sessionId, setTravelPlan });

    return (
        <>
            <Flex gap="small" style={{ minHeight: 'auto' }}>
                {showOriginCard && (
                    <Card size="small" style={{ padding: '8px 12px', boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)", }}>
                        <Flex vertical gap="extra-small">
                            <Text type="secondary" style={{ fontSize: '12px' }}>From</Text>
                            <Text strong style={{ fontSize: '20px' }}>{travelPlan?.origin}</Text>
                            <Text style={{ fontSize: '14px' }}>{formatDate(travelPlan?.startDate)}</Text>
                        </Flex>
                    </Card>
                )}
                {showArrow && <ArrowRightOutlined />}
                {showDestinationCard && (
                    <Card size="small" style={{ padding: '8px 12px', boxShadow: "0 4px 8px rgba(0, 0, 0, 0.1)", }}>
                        <Flex vertical>
                            <Text type="secondary" style={{ fontSize: '12px' }}>To</Text>
                            <Text strong style={{ fontSize: '20px' }}>{travelPlan?.destination}</Text>
                            <Text style={{ fontSize: '14px' }}>{formatDate(travelPlan?.endDate)}</Text>
                        </Flex>
                    </Card>
                )}
            </Flex>
            <div>
                {travelPlan?.flightPlan?.flightOption && (
                    <Flight flight={travelPlan.flightPlan.flightOption} />
                )}
            </div>
        </>
    );
}
export default TravelPlan;