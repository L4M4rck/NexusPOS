import { AlertCircle, PackageOpen } from 'lucide-react'

export function LoadingState({ label = 'Cargando información...' }: { label?: string }) {
  return <div className="state-panel" role="status"><span className="spinner" />{label}</div>
}

export function ErrorState({ message, retry }: { message: string; retry?: () => void }) {
  return (
    <div className="state-panel state-error" role="alert">
      <AlertCircle size={28} />
      <p>{message}</p>
      {retry && <button className="button button-secondary" onClick={retry}>Intentar de nuevo</button>}
    </div>
  )
}

export function EmptyState({ message }: { message: string }) {
  return <div className="state-panel"><PackageOpen size={34} /><p>{message}</p></div>
}
