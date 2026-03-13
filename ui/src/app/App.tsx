import './App.css'
import { useEffect } from 'react';
import { Layout } from 'antd';
import ChatPage from '../pages/ChatPage';
import RootHeader from './layout/RootHeader';
import RootNavigationMenu from './layout/RootNavigationMenu';
import ErrorBoundary from './ErrorBoundary';
import { createSession } from './services/sessionService';
import { useSessionStore } from './store/sessionStore';

const { Header, Content, Sider } = Layout;

function App() {
  const setSessionId = useSessionStore((s) => s.setSessionId);

  useEffect(() => {
    createSession().then((session) => setSessionId(session.id));
  }, [setSessionId]);

  return (
    <ErrorBoundary>
      <Layout className="app-layout">
        <Header className="app-header">
          <RootHeader />
        </Header>
        <Layout className="app-body">
          <Sider width={250} className="app-sider"><RootNavigationMenu /></Sider>
          <Content className="app-content">
            <ChatPage />
          </Content>
        </Layout>
      </Layout>
    </ErrorBoundary>
  )
}

export default App
