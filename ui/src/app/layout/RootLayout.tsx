import { Flex, Layout } from "antd"
import { useEffect, useState } from "react";
import styles from './RootLayout.module.css';
import TravelPlan from "../../features/travel-planning/components/plan/TravelPlan";

import RootHeader from "./RootHeader";
import ChatOutput from "../../features/chat/components/ChatOutput";
import NavigationHeader from "./NavigationHeader";
import TravelOptions from "../../features/travel-planning/components/plan/TravelOptions";
import Chat from "../../features/chat/components/Chat";
import { ChatService } from "../../features/chat/api/chat.api";

const { Header, Sider, Content } = Layout;

const RootLayout = () => {
    const [sessionId, setSessionId] = useState<string>("");

    useEffect(() => {
        const chatService = new ChatService();
        chatService.createSession().then(session => {

            setSessionId(session.threadId);
            console.log("Session created:", session.threadId);
        }).catch(error => {
            console.error("Failed to create session:", error);
        });
    }, []);

    const [collapsed, setCollapsed] = useState(true);

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
                            <TravelPlan sessionId={sessionId} />
                        </Flex>

                        <TravelOptions sessionId={sessionId} />

                        <Chat sessionId={sessionId} />
                    </Flex>



                </Content>
                <Sider className={styles.statusSidebar} collapsible collapsed={collapsed} trigger={null} width={550}>
                    {!collapsed && (
                        <ChatOutput />
                    )}

                </Sider>
            </Layout>
        </Layout>

    </>

}

export default RootLayout