import { Menu } from 'antd';
import { PlusOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { createTravelPlan } from '../../features/travel/services/travelPlanService';
import './layout.css';

const RootNavigationMenu = () => {
    const navigate = useNavigate();

    const handleNewPlan = async () => {
        const plan = await createTravelPlan();
        navigate(`/travel-plans/${plan.id}`);
    };

    return (
        <Menu
            mode="inline"
            defaultSelectedKeys={['chat']}
            className="nav-menu"
            items={[
                {
                    key: 'new-travel-plan',
                    label: 'New Travel Plan',
                    icon: <PlusOutlined />,
                    onClick: handleNewPlan,
                },
            ]}
        />
    );
};

export default RootNavigationMenu;
