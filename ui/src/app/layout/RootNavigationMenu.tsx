import { Menu } from 'antd';
import { useTravelPlanStore } from '../../features/travel/store/travelPlanStore';

const RootNavigationMenu = () => {
    const createPlan = useTravelPlanStore((s) => s.createPlan);

    return (
        <Menu
            mode="inline"
            defaultSelectedKeys={['chat']}
            style={{ height: '100%', borderRight: 0, background: 'transparent' }}
            items={[

                {
                    key: 'new-travel-plan',
                    label: 'New Travel Plan',
                    onClick: createPlan,
                },
            ]}
        />
    );
};

export default RootNavigationMenu;
