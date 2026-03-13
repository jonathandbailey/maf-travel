import './App.css'
import { Layout } from 'antd';
import { Navigate, Route, Routes } from 'react-router-dom';
import ChatPage from '../pages/ChatPage';
import DashboardPage from '../pages/DashboardPage';
import RootHeader from './layout/RootHeader';
import RootNavigationMenu from './layout/RootNavigationMenu';
import ErrorBoundary from './ErrorBoundary';

const { Header, Content, Sider } = Layout;

function App() {
  return (
    <ErrorBoundary>
      <Layout className="app-layout">
        <Header className="app-header">
          <RootHeader />
        </Header>
        <Layout className="app-body">
          <Sider width={250} className="app-sider"><RootNavigationMenu /></Sider>
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
