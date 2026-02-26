import { useState } from 'react';
import './App.css'
import { Input, Layout } from 'antd';

const { Header, Content } = Layout;

function App() {

  const [inputValue, setInputValue] = useState("");
  const handleKeyDown = async (e: React.KeyboardEvent<HTMLInputElement>) => { }

  return (
    <>
      <Layout>
        <Header style={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', padding: '0 16px', background: 'white', borderBottom: '1px solid #e0e0e0', flexShrink: 0 }}>

        </Header>
        <Layout style={{ flex: 1, minHeight: 0, overflow: 'hidden' }}>
          <Content style={{
            height: '100%',
            background: 'white',
            minHeight: 0, overflow: 'hidden', padding: '16px'
          }}>
            <div style={{ display: "flex", flexDirection: "column", height: "100%", overflow: "hidden", alignItems: "center" }}>
              <div style={{ flex: 1, overflow: "auto", width: "100%", maxWidth: 768 }}>

              </div>
              <Input
                placeholder="Where would you like to go today?"
                style={{ flexShrink: 0, width: "100%", maxWidth: 768 }}
                value={inputValue}
                onChange={(e) => setInputValue(e.target.value)}
                onKeyDown={handleKeyDown}
              />
            </div>

          </Content>
        </Layout>
      </Layout>
    </>
  )
}

export default App
