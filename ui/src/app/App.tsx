import './App.css'
import { useState, useEffect } from 'react';
import { Button, Layout } from 'antd';
import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons';
import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import ChatPage from '../pages/ChatPage';
import DashboardPage from '../pages/DashboardPage';
import RootHeader from './layout/RootHeader';
import RootNavigationMenu from './layout/RootNavigationMenu';
import ErrorBoundary from './ErrorBoundary';

const { Header, Content, Sider } = Layout;

function App() {
  const location = useLocation();
  const [collapsed, setCollapsed] = useState(location.pathname === '/');

  useEffect(() => {
    setCollapsed(location.pathname === '/');
  }, [location.pathname]);

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
            />
            <RootNavigationMenu collapsed={collapsed} />
          </Sider>
          <Content className="app-content">
            <Routes>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/travel-plans/:id" element={<ChatPage />} />
              <Route path="*" element={<Navigate to="/" />} />
            </Routes>
          </Content>
        </Layout>
      </Layout>
    </ErrorBoundary>
  )
}

export default App
