// Actions sur une saisie existante : consommer un stock, confirmer une suppression.
import { useState, type ReactNode } from 'react'
import { api } from '../api/client'
import type { Stock } from '../api/types'
import { Champ, FormulairePanneau, Panneau, useEnvoi, versNombre } from '../components/Formulaire'
import { Jauge } from '../components/Page'
import { dateLisible, nombre } from '../outils'

// ===========================================================================
// Consommer un stock (aliment distribué, médicament utilisé…)
// ===========================================================================
export function FormulaireConsommation({
  stock,
  onFermer,
  onFait,
}: {
  stock: Stock | null
  onFermer: () => void
  onFait: (stock: Stock) => void
}) {
  return (
    <Panneau
      ouvert={stock !== null}
      onFermer={onFermer}
      titre="Consommer du stock"
      sousTitre={stock ? `${stock.nom}${stock.bandeCode ? ` · ${stock.bandeCode}` : ''}` : undefined}
    >
      {stock && <CorpsConsommation key={stock.id} stock={stock} onFermer={onFermer} onFait={onFait} />}
    </Panneau>
  )
}

function CorpsConsommation({ stock, onFermer, onFait }: { stock: Stock; onFermer: () => void; onFait: (stock: Stock) => void }) {
  const [quantite, setQuantite] = useState('')
  const { envoi, erreur, champs, envoyer, signaler } = useEnvoi()
  const q = versNombre(quantite) ?? 0
  const reste = Math.max(0, stock.quantiteActuelle - q)
  const unite = stock.uniteMesureLibelle

  // Raccourcis : quantités usuelles, sans dépasser ce qui reste
  const raccourcis = [1, 5, 10, 25, 50].filter((n) => n < stock.quantiteActuelle)

  const valider = () => {
    if (!q || q <= 0) return signaler('quantiteConsommee', 'Indiquez la quantité utilisée.')
    if (q > stock.quantiteActuelle) return signaler('quantiteConsommee', `Il ne reste que ${nombre(stock.quantiteActuelle, 2)} ${unite}.`)
    void envoyer(async () => {
      const apres = await api.post<Stock>(`/Stocks/${stock.id}/consommer`, { stockId: stock.id, quantiteConsommee: q })
      onFait(apres)
    })
  }

  return (
    <FormulairePanneau
      onSubmit={valider}
      erreur={erreur}
      envoi={envoi}
      libelleValider="Enregistrer la consommation"
      onAnnuler={onFermer}
      resume={
        <>
          <span className="discret">Restera</span>
          <strong className={`montant ${q > stock.quantiteActuelle ? 'texte-danger' : ''}`}>
            {nombre(reste, 2)} {unite}
          </strong>
        </>
      }
    >
      {stock.estPerime && (
        <div className="encart-info encart-danger">
          <strong>Produit périmé</strong>
          <p>Ce stock est périmé depuis le {stock.datePeremption ? dateLisible(stock.datePeremption) : '—'} : l’API refusera de l’utiliser.</p>
        </div>
      )}
      <div className="etat-stock">
        <div className="etat-stock-texte">
          <span className="discret">Disponible</span>
          <strong>
            {nombre(stock.quantiteActuelle, 2)} <span className="discret">/ {nombre(stock.quantiteInitiale, 2)} {unite}</span>
          </strong>
        </div>
        <Jauge valeur={stock.quantiteActuelle} max={stock.quantiteInitiale} ton={stock.estEnAlerte ? 'danger' : 'vert'} />
      </div>

      <Champ libelle={`Quantité utilisée (${unite})`} obligatoire erreurs={champs.quantiteConsommee}>
        <input type="text" inputMode="decimal" value={quantite} onChange={(e) => setQuantite(e.target.value)} placeholder="Ex. : 25" />
      </Champ>
      <div className="choix-rapides">
        {raccourcis.map((n) => (
          <button key={n} type="button" className="pastille-choix" onClick={() => setQuantite(String(n))}>
            {n} {unite}
          </button>
        ))}
        <button type="button" className="pastille-choix" onClick={() => setQuantite(String(stock.quantiteActuelle).replace('.', ','))}>
          Tout ({nombre(stock.quantiteActuelle, 2)})
        </button>
      </div>
    </FormulairePanneau>
  )
}

// ===========================================================================
// Confirmation avant suppression
// ===========================================================================
export interface DemandeSuppression {
  titre: string
  description: ReactNode
  route: string
  /** Conséquence expliquée à l'utilisateur (ex. le stock est remis à jour) */
  consequence?: string
}

export function ConfirmationSuppression({
  demande,
  onFermer,
  onFait,
}: {
  demande: DemandeSuppression | null
  onFermer: () => void
  onFait: () => void
}) {
  return (
    <Panneau ouvert={demande !== null} onFermer={onFermer} titre={demande?.titre ?? 'Supprimer'}>
      {demande && <CorpsSuppression key={demande.route} demande={demande} onFermer={onFermer} onFait={onFait} />}
    </Panneau>
  )
}

function CorpsSuppression({ demande, onFermer, onFait }: { demande: DemandeSuppression; onFermer: () => void; onFait: () => void }) {
  const { envoi, erreur, envoyer } = useEnvoi()
  const valider = () =>
    void envoyer(async () => {
      await api.supprimer(demande.route)
      onFait()
    })

  return (
    <FormulairePanneau onSubmit={valider} erreur={erreur} envoi={envoi} libelleValider="Supprimer définitivement" onAnnuler={onFermer} danger>
      <div className="encart-info encart-danger">
        <strong>Cette action est définitive</strong>
        <p>{demande.consequence ?? 'L’élément sera supprimé et ne pourra pas être récupéré.'}</p>
      </div>
      <div className="recap-suppression">{demande.description}</div>
    </FormulairePanneau>
  )
}
