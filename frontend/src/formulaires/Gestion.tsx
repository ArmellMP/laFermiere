// Tableaux « gérés » : la liste + les actions sur chaque ligne (modifier, supprimer,
// consommer) + les panneaux correspondants. Utilisés dans les écrans de liste et dans
// les onglets de la fiche d'une bande. Les boutons n'apparaissent que si l'API autorise le rôle.
import { useState, type ReactNode } from 'react'
import type { Achat, Stock, Vente } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { useNotifier } from '../components/Formulaire'
import { Tableau, type Colonne } from '../components/Tableau'
import { dateLisible, monnaie, nombre } from '../outils'
import { ConfirmationSuppression, FormulaireConsommation, type DemandeSuppression } from './Actions'
import { FormulaireAchat, FormulaireVente } from './Formulaires'

export function BoutonsLigne({ children }: { children: ReactNode }) {
  return <div className="boutons-ligne">{children}</div>
}

export function BoutonLigne({ libelle, onClick, danger = false }: { libelle: string; onClick: () => void; danger?: boolean }) {
  return (
    <button type="button" className={`bouton-ligne ${danger ? 'danger' : ''}`} onClick={onClick}>
      {libelle}
    </button>
  )
}

interface PropsTableau<T> {
  lignes: T[]
  colonnes: Colonne<T>[]
  lien?: (ligne: T) => string
  vide?: ReactNode
  /** Appelé après une modification, une suppression ou une consommation */
  onChange: () => void
}

// ---------------------------------------------------------------------------
// Ventes : modifier, supprimer (Admin, Comptable)
// ---------------------------------------------------------------------------
export function TableauVentes({ lignes, colonnes, lien, vide, onChange }: PropsTableau<Vente>) {
  const { aLeRole } = useAuth()
  const notifier = useNotifier()
  const [edition, setEdition] = useState<Vente | null>(null)
  const [suppression, setSuppression] = useState<DemandeSuppression | null>(null)
  const gere = aLeRole('Admin', 'Comptable')

  return (
    <>
      <Tableau
        lignes={lignes}
        colonnes={colonnes}
        cle={(v) => v.id}
        lien={lien}
        triInitial={{ cle: 'date', sens: 'desc' }}
        vide={vide}
        actions={
          gere
            ? (v) => (
                <BoutonsLigne>
                  <BoutonLigne libelle="Modifier" onClick={() => setEdition(v)} />
                  <BoutonLigne
                    libelle="Supprimer"
                    danger
                    onClick={() =>
                      setSuppression({
                        titre: 'Supprimer la vente',
                        route: `/Ventes/${v.id}`,
                        consequence: v.typeVente.startsWith('Poulets')
                          ? `Les ${nombre(v.quantite)} poulets vendus seront remis dans l’effectif de la bande.`
                          : undefined,
                        description: (
                          <>
                            <strong>{monnaie(v.montantTotal)}</strong> · {v.clientNom} · {v.typeVenteLibelle} · {dateLisible(v.dateVente)}
                          </>
                        ),
                      })
                    }
                  />
                </BoutonsLigne>
              )
            : undefined
        }
      />
      <FormulaireVente
        ouvert={edition !== null}
        existant={edition ?? undefined}
        onFermer={() => setEdition(null)}
        onCree={(v) => {
          setEdition(null)
          notifier(`Vente modifiée : ${monnaie(v.montantTotal)}`)
          onChange()
        }}
      />
      <ConfirmationSuppression
        demande={suppression}
        onFermer={() => setSuppression(null)}
        onFait={() => {
          setSuppression(null)
          notifier('Vente supprimée')
          onChange()
        }}
      />
    </>
  )
}

// ---------------------------------------------------------------------------
// Achats : modifier, supprimer (Admin, Comptable) — le stock suit (triggers SQL)
// ---------------------------------------------------------------------------
export function TableauAchats({ lignes, colonnes, lien, vide, onChange }: PropsTableau<Achat>) {
  const { aLeRole } = useAuth()
  const notifier = useNotifier()
  const [edition, setEdition] = useState<Achat | null>(null)
  const [suppression, setSuppression] = useState<DemandeSuppression | null>(null)
  const gere = aLeRole('Admin', 'Comptable')

  return (
    <>
      <Tableau
        lignes={lignes}
        colonnes={colonnes}
        cle={(a) => a.id}
        lien={lien}
        triInitial={{ cle: 'date', sens: 'desc' }}
        vide={vide}
        actions={
          gere
            ? (a) => (
                <BoutonsLigne>
                  <BoutonLigne libelle="Modifier" onClick={() => setEdition(a)} />
                  <BoutonLigne
                    libelle="Supprimer"
                    danger
                    onClick={() =>
                      setSuppression({
                        titre: 'Supprimer l’achat',
                        route: `/Achats/${a.id}`,
                        consequence:
                          a.typeAchat === 'Poussins'
                            ? `Les ${nombre(a.quantite)} poussins achetés seront retirés de la bande.`
                            : ['Aliment', 'Medicament', 'Materiel', 'Autre'].includes(a.typeAchat)
                              ? 'La quantité achetée sera retirée du stock. Refusé si elle a déjà été consommée.'
                              : undefined,
                        description: (
                          <>
                            <strong>{monnaie(a.montantTotal)}</strong> · {a.fournisseurNom} · {a.typeAchatLibelle} · {dateLisible(a.dateAchat)}
                          </>
                        ),
                      })
                    }
                  />
                </BoutonsLigne>
              )
            : undefined
        }
      />
      <FormulaireAchat
        ouvert={edition !== null}
        existant={edition ?? undefined}
        onFermer={() => setEdition(null)}
        onCree={(a) => {
          setEdition(null)
          notifier(`Achat modifié : ${monnaie(a.montantTotal)}`)
          onChange()
        }}
      />
      <ConfirmationSuppression
        demande={suppression}
        onFermer={() => setSuppression(null)}
        onFait={() => {
          setSuppression(null)
          notifier('Achat supprimé')
          onChange()
        }}
      />
    </>
  )
}

// ---------------------------------------------------------------------------
// Stocks : consommer (Admin, Fermier)
// ---------------------------------------------------------------------------
export function TableauStocks({ lignes, colonnes, lien, vide, onChange }: PropsTableau<Stock>) {
  const { aLeRole } = useAuth()
  const notifier = useNotifier()
  const [aConsommer, setAConsommer] = useState<Stock | null>(null)
  const gere = aLeRole('Admin', 'Fermier')

  return (
    <>
      <Tableau
        lignes={lignes}
        colonnes={colonnes}
        cle={(s) => s.id}
        lien={lien}
        triInitial={{ cle: 'etat', sens: 'desc' }}
        vide={vide}
        actions={
          gere
            ? (s) =>
                s.quantiteActuelle > 0 && !s.estPerime ? (
                  <BoutonsLigne>
                    <BoutonLigne libelle="Consommer" onClick={() => setAConsommer(s)} />
                  </BoutonsLigne>
                ) : null
            : undefined
        }
      />
      <FormulaireConsommation
        stock={aConsommer}
        onFermer={() => setAConsommer(null)}
        onFait={(s) => {
          setAConsommer(null)
          notifier(`${s.nom} : il reste ${nombre(s.quantiteActuelle, 2)} ${s.uniteMesureLibelle}`)
          onChange()
        }}
      />
    </>
  )
}
