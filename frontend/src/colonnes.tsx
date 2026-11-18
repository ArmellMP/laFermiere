// Colonnes des tableaux de stocks, ventes et achats : partagées entre les écrans
// de liste (Stocks, Ventes, Achats) et les onglets de la fiche d'une bande.
import type { Achat, Stock, Vente } from './api/types'
import { Badge, Jauge } from './components/Page'
import type { Colonne } from './components/Tableau'
import { dateLisible, monnaie, nombre } from './outils'

export const colonnesStock: Colonne<Stock>[] = [
  { cle: 'nom', titre: 'Produit', rendu: (s) => <strong>{s.nom}</strong>, tri: (s) => s.nom, mobile: 'titre' },
  { cle: 'type', titre: 'Type', rendu: (s) => s.typeLibelle, tri: (s) => s.typeLibelle, mobile: 'sous-titre' },
  {
    cle: 'quantite',
    titre: 'Quantité restante',
    rendu: (s) => (
      <span className="cellule-cycle">
        <span>
          {nombre(s.quantiteActuelle, 2)} <span className="discret">/ {nombre(s.quantiteInitiale, 2)} {s.uniteMesureLibelle}</span>
        </span>
        <Jauge valeur={s.quantiteActuelle} max={s.quantiteInitiale} ton={s.estEnAlerte ? 'danger' : 'vert'} />
      </span>
    ),
    tri: (s) => (s.quantiteInitiale ? s.quantiteActuelle / s.quantiteInitiale : 0),
    mobile: 'detail',
  },
  {
    cle: 'etat',
    titre: 'État',
    rendu: (s) =>
      s.estPerime ? <Badge ton="danger">Périmé</Badge> : s.estEnAlerte ? <Badge ton="soleil">Stock bas</Badge> : <Badge ton="vert">Disponible</Badge>,
    tri: (s) => (s.estPerime ? 2 : s.estEnAlerte ? 1 : 0),
    mobile: 'droite',
  },
  {
    cle: 'peremption',
    titre: 'Péremption',
    rendu: (s) => (s.datePeremption ? dateLisible(s.datePeremption) : <span className="discret">—</span>),
    tri: (s) => s.datePeremption ?? '9999',
    mobile: 'detail',
  },
  { cle: 'valeur', titre: 'Valeur', rendu: (s) => monnaie(s.valeurStock), tri: (s) => s.valeurStock, alignement: 'droite', mobile: 'detail' },
]

export const colonnesVente: Colonne<Vente>[] = [
  { cle: 'date', titre: 'Date', rendu: (v) => dateLisible(v.dateVente), tri: (v) => v.dateVente, mobile: 'sous-titre' },
  { cle: 'client', titre: 'Client', rendu: (v) => <strong>{v.clientNom ?? '—'}</strong>, tri: (v) => v.clientNom ?? '', mobile: 'titre' },
  { cle: 'bande', titre: 'Bande', rendu: (v) => v.bandeCode ?? '—', tri: (v) => v.bandeCode ?? '', mobile: 'detail' },
  { cle: 'type', titre: 'Produit', rendu: (v) => v.typeVenteLibelle, tri: (v) => v.typeVenteLibelle, mobile: 'detail' },
  { cle: 'quantite', titre: 'Quantité', rendu: (v) => `${nombre(v.quantite, 2)} ${v.uniteMesureLibelle}`, tri: (v) => v.quantite, alignement: 'droite', mobile: 'detail' },
  { cle: 'montant', titre: 'Montant', rendu: (v) => <strong>{monnaie(v.montantTotal)}</strong>, tri: (v) => v.montantTotal, alignement: 'droite', mobile: 'droite' },
  {
    cle: 'paiement',
    titre: 'Paiement',
    rendu: (v) => (v.estPaye ? <Badge ton="vert">Payée</Badge> : <Badge ton="danger">À encaisser</Badge>),
    tri: (v) => Number(v.estPaye),
    mobile: 'detail',
  },
]

export const colonnesAchat: Colonne<Achat>[] = [
  { cle: 'date', titre: 'Date', rendu: (a) => dateLisible(a.dateAchat), tri: (a) => a.dateAchat, mobile: 'sous-titre' },
  { cle: 'fournisseur', titre: 'Fournisseur', rendu: (a) => <strong>{a.fournisseurNom ?? '—'}</strong>, tri: (a) => a.fournisseurNom ?? '', mobile: 'titre' },
  { cle: 'bande', titre: 'Bande', rendu: (a) => a.bandeCode ?? '—', tri: (a) => a.bandeCode ?? '', mobile: 'detail' },
  { cle: 'type', titre: 'Type', rendu: (a) => a.typeAchatLibelle, tri: (a) => a.typeAchatLibelle, mobile: 'detail' },
  { cle: 'quantite', titre: 'Quantité', rendu: (a) => `${nombre(a.quantite, 2)} ${a.uniteMesureLibelle}`, tri: (a) => a.quantite, alignement: 'droite', mobile: 'detail' },
  { cle: 'montant', titre: 'Montant', rendu: (a) => <strong>{monnaie(a.montantTotal)}</strong>, tri: (a) => a.montantTotal, alignement: 'droite', mobile: 'droite' },
  {
    cle: 'recu',
    titre: 'Réception',
    rendu: (a) => (a.estRecu ? <Badge ton="vert">Reçu</Badge> : <Badge ton="soleil">En attente</Badge>),
    tri: (a) => Number(a.estRecu),
    mobile: 'detail',
  },
]
