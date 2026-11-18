import { useMemo, useState } from 'react'
import { api } from '../api/client'
import { avecNomsStocks } from '../api/noms'
import type { Stock, TypeStock } from '../api/types'
import { colonnesStock } from '../colonnes'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { IconeAlerte, IconeCarton, IconeHorloge, IconeRecherche } from '../components/Icones'
import { BarreOutils, EnTetePage, Indicateurs, Puces, Recherche, Vide, useEtatUrl } from '../components/Page'
import { Tableau, type Colonne } from '../components/Tableau'
import { monnaieCompacte, normaliser } from '../outils'

type Etat = 'tous' | 'alerte' | 'perimes' | 'disponibles'

const TYPES: { valeur: 'tous' | TypeStock; libelle: string }[] = [
  { valeur: 'tous', libelle: 'Tous types' },
  { valeur: 'Aliment', libelle: 'Aliment' },
  { valeur: 'Medicament', libelle: 'Médicament' },
  { valeur: 'Vaccin', libelle: 'Vaccin' },
  { valeur: 'Materiel', libelle: 'Matériel' },
  { valeur: 'Autre', libelle: 'Autre' },
]

const colonnes: Colonne<Stock>[] = [
  ...colonnesStock.slice(0, 2),
  { cle: 'bande', titre: 'Bande', rendu: (s) => s.bandeCode ?? '—', tri: (s) => s.bandeCode ?? '', mobile: 'detail' },
  ...colonnesStock.slice(2),
]

export default function Stocks() {
  const [etat, setEtat] = useEtatUrl<Etat>('etat', 'tous')
  const [type, setType] = useEtatUrl<'tous' | TypeStock>('type', 'tous')
  const [recherche, setRecherche] = useState('')
  const { donnees, erreur, enCours, recharger } = useDonnees(async () => avecNomsStocks(await api.liste<Stock>('/Stocks')), [])
  const stocks = useMemo(() => donnees ?? [], [donnees])

  const filtres = useMemo(() => {
    const q = normaliser(recherche.trim())
    return stocks
      .filter((s) => (etat === 'alerte' ? s.estEnAlerte : etat === 'perimes' ? s.estPerime : etat === 'disponibles' ? !s.estEnAlerte && !s.estPerime : true))
      .filter((s) => type === 'tous' || s.type === type)
      .filter((s) => !q || normaliser(`${s.nom} ${s.bandeCode ?? ''} ${s.fournisseurNom ?? ''}`).includes(q))
  }, [stocks, etat, type, recherche])

  const enAlerte = stocks.filter((s) => s.estEnAlerte).length
  const perimes = stocks.filter((s) => s.estPerime).length
  const bientot = stocks.filter((s) => !s.estPerime && s.datePeremption && s.joursAvantPeremption <= 15).length
  const valeur = stocks.reduce((t, s) => t + s.valeurStock, 0)

  return (
    <main className="page">
      <EnTetePage
        miettes={[{ libelle: 'Tableau de bord', vers: '/' }, { libelle: 'Élevage' }, { libelle: 'Stocks' }]}
        titre="Stocks"
        sousTitre="Aliment, médicaments, vaccins et matériel de chaque bande."
      />

      <Indicateurs
        elements={[
          { libelle: 'Valeur des stocks', valeur, format: monnaieCompacte, icone: <IconeCarton taille={20} />, ton: 'vert', detail: `${stocks.length} article${stocks.length > 1 ? 's' : ''}` },
          { libelle: 'Stocks bas', valeur: enAlerte, icone: <IconeAlerte taille={20} />, ton: enAlerte ? 'soleil' : 'neutre', detail: 'sous le seuil d’alerte' },
          { libelle: 'Périmés', valeur: perimes, icone: <IconeAlerte taille={20} />, ton: perimes ? 'danger' : 'neutre', detail: 'à retirer' },
          { libelle: 'Périment bientôt', valeur: bientot, icone: <IconeHorloge taille={20} />, ton: bientot ? 'soleil' : 'neutre', detail: 'dans 15 jours ou moins' },
        ]}
      />

      <BarreOutils>
        <Puces
          libelle="Filtrer par état"
          valeur={etat}
          onChange={setEtat}
          options={[
            { valeur: 'tous', libelle: 'Tous', compte: stocks.length },
            { valeur: 'alerte', libelle: 'Stock bas', compte: enAlerte, ton: 'soleil' },
            { valeur: 'perimes', libelle: 'Périmés', compte: perimes, ton: 'danger' },
            { valeur: 'disponibles', libelle: 'Disponibles', compte: stocks.length - enAlerte - perimes, ton: 'vert' },
          ]}
        />
        <div className="barre-outils-ligne">
          <Recherche valeur={recherche} onChange={setRecherche} placeholder="Produit, bande, fournisseur…" />
          <label className="selecteur-tri">
            <select value={type} onChange={(e) => setType(e.target.value as 'tous' | TypeStock)} aria-label="Type de stock">
              {TYPES.map((t) => (
                <option key={t.valeur} value={t.valeur}>
                  {t.libelle}
                </option>
              ))}
            </select>
          </label>
        </div>
      </BarreOutils>

      <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={3} />
      {!enCours && !erreur && (
        <>
          <p className="resultat-compte">
            {filtres.length} article{filtres.length > 1 ? 's' : ''}
          </p>
          <Tableau
            lignes={filtres}
            colonnes={colonnes}
            cle={(s) => s.id}
            lien={(s) => `/bandes/${s.bandeId}?onglet=stocks`}
            triInitial={{ cle: 'etat', sens: 'desc' }}
            vide={<Vide icone={<IconeRecherche taille={28} />} titre="Aucun stock ne correspond" texte="Changez de filtre ou de recherche." />}
          />
        </>
      )}
    </main>
  )
}
