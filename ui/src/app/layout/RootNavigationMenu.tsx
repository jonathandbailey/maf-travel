import { useEffect, useState } from 'react';
import { Menu } from 'antd';
import { PlusOutlined, EnvironmentOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { createTravelPlan, listTravelPlans } from '../../features/travel/services/travelPlanService';
import type { TravelPlanResponse } from '../../features/travel/services/travelPlanService';
import './layout.css';

const RootNavigationMenu = ({ collapsed }: { collapsed: boolean }) => {
    const navigate = useNavigate();
    const [plans, setPlans] = useState<TravelPlanResponse[]>([]);

    useEffect(() => {
        listTravelPlans().then(setPlans).catch(() => { });
    }, []);

    const handleNewPlan = async () => {
        const plan = await createTravelPlan();
        navigate(`/travel-plans/${plan.id}`);
    };

    return (
        <Menu
            mode="inline"
            inlineCollapsed={collapsed}
            defaultSelectedKeys={['chat']}
            className="nav-menu"
            items={[
                {
                    key: 'new-travel-plan',
                    label: 'New Travel Plan',
                    icon: <PlusOutlined />,
                    onClick: handleNewPlan,
                },
                { type: 'divider' },
                ...plans.map((plan) => ({
                    key: plan.id,
                    label: plan.destination ?? 'Untitled Plan',
                    icon: <EnvironmentOutlined />,
                    onClick: () => navigate(`/travel-plans/${plan.id}`),
                })),
            ]}
        />
    );
};

export default RootNavigationMenu;
