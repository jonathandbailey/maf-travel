import { Flex, Layout } from "antd"
import { useState } from "react";
import styles from './RootLayout.module.css';
import TravelPlan from "../../features/travel-planning/components/plan/TravelPlan";
import { useExchangesStore } from "../../features/chat/stores/exchanges.store";

import Welcome from "../../features/dashboard/components/Welcome";

import RootHeader from "./RootHeader";
import ChatOutput from "../../features/chat/components/ChatOutput";
import NavigationHeader from "./NavigationHeader";
import TravelOptions from "../../features/travel-planning/components/plan/TravelOptions";
import { useTravelPlanUpdateHandler } from "../../features/travel-planning/hooks/useTravelPlanUpdateHandler";
import type { TravelPlanDto } from "../../features/travel-planning/api/travel.dto";
import Chat from "../../features/chat/components/Chat";

const { Header, Sider, Content } = Layout;

const RootLayout = () => {
    const [sessionId] = useState<string>(crypto.randomUUID());
    const { exchanges } = useExchangesStore();
    const [travelPlan, setTravelPlan] = useState<TravelPlanDto | null>(null);

    const [collapsed, setCollapsed] = useState(true);

    useTravelPlanUpdateHandler({ sessionId, setTravelPlan });


    return <>

        <Layout className={styles.layout}>
            <Header className={styles.header}>
                <RootHeader />
            </Header>

            <Layout style={{ height: "calc(100vh - 64px)" }}>

                <Content style={{ background: "white", height: "100%", display: "flex", flexDirection: "column" }} >
                    <Header style={{ background: "white", padding: 0 }}>
                        <NavigationHeader collapsed={collapsed} setCollapsed={setCollapsed} />
                    </Header>
                    <Flex vertical align="center" style={{ height: "100%", flex: 1, minHeight: 0 }}>
                        <Flex justify="center" align="start">
                            <TravelPlan travelPlan={travelPlan} />
                        </Flex>

                        {exchanges.length === 0 && <Welcome />}
                        <TravelOptions sessionId={sessionId} />

                        <Chat sessionId={sessionId} />
                    </Flex>



                </Content>
                <Sider className={styles.statusSidebar} collapsible collapsed={collapsed} trigger={null} width={550}>
                    {!collapsed && (
                        <ChatOutput exchanges={exchanges} />
                    )}

                </Sider>
            </Layout>
        </Layout>

    </>

}

export default RootLayout