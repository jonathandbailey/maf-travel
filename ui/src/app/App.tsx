import './App.css'
import { lazy, Suspense, useState } from 'react';
import { Button, Layout, Spin } from 'antd';
import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons';
import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import RootHeader from './layout/RootHeader';
import RootNavigationMenu from './layout/RootNavigationMenu';
import ErrorBoundary from './ErrorBoundary';

const DashboardPage = lazy(() => import('../pages/DashboardPage'));
const ChatPage = lazy(() => import('../pages/ChatPage'));

const { Header, Content, Sider } = Layout;

function App() {
  const location = useLocation();
  const [collapsed, setCollapsed] = useState(location.pathname === '/');

  // Sync collapsed state with route changes without useEffect (React-recommended derived-state pattern)
  const [prevPathname, setPrevPathname] = useState(location.pathname);
  if (prevPathname !== location.pathname) {
    setPrevPathname(location.pathname);
    setCollapsed(location.pathname === '/');
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
        </Layout>
      </Layout>
    </ErrorBoundary>
  )
}

export default App
