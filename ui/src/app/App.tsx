import './App.css'
import { lazy, Suspense, useState } from 'react';
import { Button, Layout, Spin } from 'antd';
import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons';
import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import RootHeader from './layout/RootHeader';
import RootNavigationMenu from './layout/RootNavigationMenu';
import ErrorBoundary from './ErrorBoundary';
import Exchanges from '@/features/chat/components/Exchanges';
import { useChatStore } from '@/features/chat/store/chatStore';

const DashboardPage = lazy(() => import('../pages/DashboardPage'));
const ChatPage = lazy(() => import('../pages/ChatPage'));

const { Header, Content, Sider } = Layout;

function App() {
  const location = useLocation();
  const [collapsed, setCollapsed] = useState(location.pathname === '/');
  const [rightCollapsed, setRightCollapsed] = useState(true);

  const isChatPage = location.pathname.startsWith('/travel-plans/');
  const exchanges = useChatStore((s) => s.exchanges);

  // Sync collapsed state with route changes without useEffect (React-recommended derived-state pattern)
  const [prevPathname, setPrevPathname] = useState(location.pathname);
  if (prevPathname !== location.pathname) {
    setPrevPathname(location.pathname);
    setCollapsed(location.pathname === '/');
    if (!location.pathname.startsWith('/travel-plans/')) {
      setRightCollapsed(true);
    }
  }

  return (
    <ErrorBoundary>
      <Layout className="app-layout">
        <Header className="app-header">
          <RootHeader />
        </Header>
        <Layout className="app-body">
          <Sider width={250} collapsedWidth={48} collapsed={collapsed} className="app-sider">
            <Button
              type="text"
              icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
              onClick={() => setCollapsed(!collapsed)}
              className="sider-toggle"
              aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
            />
            <RootNavigationMenu collapsed={collapsed} />
          </Sider>
          <Content className="app-content">
            <Suspense fallback={<Spin size="large" style={{ display: 'flex', justifyContent: 'center', marginTop: 80 }} />}>
              <Routes>
                <Route path="/" element={<DashboardPage />} />
                <Route path="/travel-plans/:id" element={<ChatPage />} />
                <Route path="*" element={<Navigate to="/" />} />
              </Routes>
            </Suspense>
          </Content>
          {isChatPage && (
            <Sider
              width={520}
              collapsedWidth={48}
              collapsed={rightCollapsed}
              trigger={null}
              className="app-sider-right"
            >
              <Button
                type="text"
                icon={rightCollapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
                onClick={() => setRightCollapsed((o) => !o)}
                className="sider-toggle"
                aria-label={rightCollapsed ? "Expand sidebar" : "Collapse sidebar"}
              />
              {!rightCollapsed && exchanges.length > 0 && (
                <div className="sider-exchanges">
                  <Exchanges exchanges={exchanges} />
                </div>
              )}
            </Sider>
          )}
        </Layout>
      </Layout>
    </ErrorBoundary>
  )
}

export default App
