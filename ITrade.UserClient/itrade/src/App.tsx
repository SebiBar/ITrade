import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import { userService } from './api'

function App() {
  const [count, setCount] = useState(0)
  const [pingResponse, setPingResponse] = useState<string>('')
  const [pingLoading, setPingLoading] = useState(false)
  const [pingError, setPingError] = useState<string>('')

  const handlePing = async () => {
    setPingLoading(true)
    setPingError('')
    setPingResponse('')

    try {
      const response = await userService.ping()
      console.log('✅ Ping response:', response)
      setPingResponse(response)
    } catch (error) {
      console.error('❌ Ping error:', error)
      setPingError(error instanceof Error ? error.message : 'Ping failed')
    } finally {
      setPingLoading(false)
    }
  }

  return (
    <>
      <div>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <h1>Vite + React</h1>
      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>

      <div className="card">
        <h2>API Test</h2>
        <button onClick={handlePing} disabled={pingLoading}>
          {pingLoading ? 'Pinging...' : 'Test API Ping'}
        </button>
        {pingResponse && (
          <p style={{ color: 'green', marginTop: '10px' }}>
            ✓ Response: {pingResponse}
          </p>
        )}
        {pingError && (
          <p style={{ color: 'red', marginTop: '10px' }}>
            ✗ Error: {pingError}
          </p>
        )}
      </div>

      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>
    </>
  )
}

export default App
