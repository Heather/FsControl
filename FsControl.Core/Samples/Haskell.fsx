﻿#nowarn "3186"
#r @"..\bin\Release\FsControl.dll"

// This sample code mimics Haskell functions.
// It is based in an initial exploratory project  at http://code.google.com/p/fsharp-typeclasses/ no longer maintained.


let flip f x y = f y x
let const' k _ = k

let (</) = (|>)
let (/>) = flip
let (++) = (@)
let (==) = (=)
let (=/) x y = not (x = y)

type DeReference = DeReference with
    static member ($) (DeReference, a:'a ref     ) = !a
    static member ($) (DeReference, a:string     ) = a.ToCharArray() |> Array.toList
    static member ($) (DeReference, a:DeReference) = DeReference

let inline (!) a = DeReference $ a

type Maybe<'t> = Option<'t>
let  Just x :Maybe<'t> = Some x
let  Nothing:Maybe<'t> = None
let  (|Just|Nothing|) = function Some x -> Just x | _ -> Nothing
let maybe  n f = function | Nothing -> n | Just x -> f x

type Either<'a,'b> = Choice<'b,'a>
let  Right x :Either<'a,'b> = Choice1Of2 x
let  Left  x :Either<'a,'b> = Choice2Of2 x
let  (|Right|Left|) = function Choice1Of2 x -> Right x | Choice2Of2 x -> Left x
let either f g = function Left x -> f x | Right y -> g y

// Numerics
type Integer = bigint
open System.Numerics
open FsControl
// open FsControl.Core.Types.Ratio

let inline fromInteger  (x:Integer)   :'Num    = FsControl.Operators.fromBigInt x
let inline toInteger    (x:'Integral) :Integer = FsControl.Operators.toBigInt   x
let inline fromIntegral (x:'Integral) :'Num = (fromInteger << toInteger) x

module NumericLiteralG =
    let inline FromZero() = Zero.Invoke()
    let inline FromOne () = One.Invoke()
    let inline FromInt32  (i:int   ) = FromInt32.Invoke i
    let inline FromInt64  (i:int64 ) = FromInt64.Invoke i
    let inline FromString (i:string) = fromInteger <| BigInteger.Parse i

let inline abs    (x:'Num) :'Num = FsControl.Operators.abs    x
let inline signum (x:'Num) :'Num = FsControl.Operators.signum x

let inline (+) (a:'Num) (b:'Num) :'Num = a + b
let inline (-) (a:'Num) (b:'Num) :'Num = a - b
let inline (*) (a:'Num) (b:'Num) :'Num = a * b

let inline negate (x:'Num) :'Num = FsControl.Operators.negate x
let inline (~-)   (x:'Num) :'Num = FsControl.Operators.negate x

let inline whenIntegral a = let _ = if false then toInteger a else 0I in ()

let inline div (a:'Integral) b :'Integral =
    whenIntegral a
    let (a,b) = if b < 0G then (-a,-b) else (a,b)
    (if a < 0G then (a - b + 1G) else a) / b

let inline quot (a:'Integral) (b:'Integral) :'Integral = whenIntegral a; a / b
let inline rem  (a:'Integral) (b:'Integral) :'Integral = whenIntegral a; a % b
let inline quotRem a b :'Integral * 'Integral = whenIntegral a; FsControl.Operators.divRem a b
let inline mod'   a b :'Integral = whenIntegral a; ((a % b) + b) % b  
let inline divMod D d :'Integral * 'Integral =
    let q, r = quotRem D d
    if (r < 0G) then
        if (d > 0G) then (q - 1G, r + d)
        else             (q + 1G, r - d)
    else (q, r)

// type Rational = Ratio<Integer>

let inline G0() = fromIntegral 0
let inline G1() = fromIntegral 1

let inline gcd x y :'Integral =
    let zero = G0()
    let rec gcd' a = function
        | b when b = zero -> a
        | b -> gcd' b (rem a b)
    match(x,y) with
    | t when t = (zero,zero) -> failwith "Prelude.gcd: gcd 0 0 is undefined"
    | _                      -> gcd' (abs x) (abs y)


// let inline ratio (a:'Integral) (b:'Integral) :Ratio<'Integral> =
//     whenIntegral a
//     let zero = G0()
//     if b = zero then failwith "Ratio.%: zero denominator"
//     let (a,b) = if b < zero then (negate a, negate b) else (a, b)
//     let gcd = gcd a b
//     Ratio (quot a gcd, quot b gcd)
// 
// let inline (%) (a:'Integral) (b:'Integral) :Ratio<'Integral> = a </ratio/> b
// let inline fromRational (x:Rational) :'Fractional = FsControl.Operators.fromRational x
// let inline whenFractional a = let _ = if false then fromRational (1I % 1I) else a in ()
let inline (/) (a:'Fractional) (b:'Fractional) :'Fractional = (* whenFractional a;*) a / b
let inline recip x :'Fractional = 1G / x

// Exp functions
let inline ( **^ ) (x:'Num) (n:'Integral)  = 
    whenIntegral n
    let rec f a b n = if n == 0G then a else f (b * a) b (n - 1G)
    if (n < 0G) then failwith "Negative exponent" else f 1G x n
let inline ( **^^ ) (x:'Fractional) (n:'Integral) = if n >= 0G then x**^n else recip (x**^(negate n))

// let inline properFraction (x:'RealFrac) : 'Integral * 'RealFrac =
//     let (a, b:'RealFrac) = FsControl.Operators.properFraction x
//     (fromIntegral a, b)

// let inline truncate (x:'RealFrac) :'Integral = fst <| properFraction x
// let inline toRational (x:'Real) :Rational = FsControl.Operators.toRational x
let inline pi() :'Floating = FsControl.Operators.pi ()

let inline ( **) a (b:'Floating) :'Floating = a ** b
let inline sqrt    (x:'Floating) :'Floating = sqrt x

let inline asinh x :'Floating = log (x + sqrt (1G+x*x))
let inline acosh x :'Floating = log (x + (x+1G) * sqrt ((x-1G)/(x+1G)))
let inline atanh x :'Floating = (1G/2G) * log ((1G+x) / (1G-x))

let inline logBase x y  :'Floating =  log y / log x


// Test Numerics
// let res5_55:Integer * _ = properFraction 5.55M
// let res111_20 = toRational 5.55
// let res4_3    = toRational (12 % 9)
// let res17_1   = toRational 17uy
let divisions = List.map ( quot/> 5G) [5;8;10;15;20]

let inline quadratic a b c =
    let root1 = ( -b + sqrt (  b ** 2G - 4G * a * c) )  / (2G * a)
    let root2 = ( -b - sqrt (  b ** 2G - 4G * a * c) )  / (2G * a)
    (root1,root2)

let res30_15  = quadratic 2.0  -3G -9G
let res30_15f = quadratic 2.0f -3G -9G
let resCmplx:System.Numerics.Complex * _ = quadratic 2G -3G 9G



// Monads

let inline return' x = FsControl.Operators.result x
let inline (>>=) x (f:_->'R) : 'R = FsControl.Operators.(>>=) x f
let inline join (x:'Monad'Monad'a) : 'Monad'a = FsControl.Operators.join x

let inline sequence ms =
    let k m m' = m >>= fun (x:'a) -> m' >>= fun xs -> (return' :list<'a> -> 'M) (List.Cons(x,xs))
    List.foldBack k ms ((return' :list<'a> -> 'M) [])

let inline mapM f as' = sequence (List.map f as')
let inline liftM  f m1    = m1 >>= (return' << f)
let inline liftM2 f m1 m2 = m1 >>= fun x1 -> m2 >>= fun x2 -> return' (f x1 x2)
let inline ap     x y     = liftM2 id x y
let inline (>=>)  f g x   = f x >>= g
let inline (<=<)  g f x   = f x >>= g

type DoNotationBuilder() =
    member inline b.Return(x)    = return' x
    member inline b.Bind(p,rest) = p >>= rest
    member        b.Let (p,rest) = rest p
    member    b.ReturnFrom(expr) = expr
let do' = new DoNotationBuilder()


// Test return
let resSome2 :option<_> = return' 2
let resSing2 :list<_>   = return' 2
let resLazy2 :Lazy<_>   = return' 2


// Test List Monad
// F#                           // Haskell
let result = 
    do' {                       // do {
        let! x1 = [1;2]         //   x1 <- [1;2]
        let! x2 = [10;20]       //   x2 <- [10;20]
        return ((+) x1 x2) }    //   return ((+) x1 x2) }

// desugared version
let lst11n21n12n22 = [1;2]  >>= (fun x1 -> [10;20] >>= (fun x2 ->  return'((+) x1 x2 )))


// IO
type IO<'a> = Async<'a>
let runIO  f   = Async.RunSynchronously f
let getLine    = async { return System.Console.ReadLine() }
let putStrLn x = async { printfn "%s" x}
let print    x = async { printfn "%A" x}

// Test IO
let action = do' {
    do! putStrLn  "What is your first name?"
    let! fn = getLine
    do! putStrLn  ("Thanks, " + fn) 
    do! putStrLn  ("What is your last name?")
    let! ln = getLine
    let  fullname = fn + " " + ln
    do! putStrLn  ("Your full name is: " + fullname)
    return fullname }
// try -> runIO action ;;


// Functors

let inline fmap   f x = FsControl.Operators.map f x

// Test Functors
let times2,minus3 = (*) 2, (-)/> 3
let resJust1      = fmap minus3 (Some 4G)
let noValue       = fmap minus3 None
let lstTimes2     = fmap times2 [1;2;3;4]
let fTimes2minus3 = fmap minus3 times2
let res39         = fTimes2minus3 21G
let getChars      = fmap (fun (x:string) -> x.ToCharArray() |> Seq.toList ) action
// try -> runIO getChars ;;

// Define a type Tree
type Tree<'a> =
    | Tree of 'a * Tree<'a> * Tree<'a>
    | Leaf of 'a
    static member map f (t:Tree<'a>  )  =
        match t with
        | Leaf x -> Leaf (f x)
        | Tree(x,t1,t2) -> Tree(f x, Tree.map f t1, Tree.map f t2)

// add ìnstance for Functor class
    static member Map (x:Tree<_>, f, _:Map) = Tree.map f x

let myTree = Tree(6, Tree(2, Leaf 1, Leaf 3), Leaf 9)
let mappedTree = fmap fTimes2minus3 myTree


// Comonads

let inline internal extend g s  = FsControl.Operators.extend g s
let inline internal duplicate x = FsControl.Operators.duplicate x
let inline internal (=>>)  s g  = fmap g (duplicate s)

let ct1 = duplicate [1;2;3;4] // val it : List<List<int>> = [[1; 2; 3; 4]; [2; 3; 4]; [3; 4]; [4]]
let ct2 = duplicate ("a", 10) // val it : string * (string * int) = ("a", ("a", 10))
let ct3 = duplicate (fun (x:string) -> System.Int32.Parse x) // r3 "80" "100"  val it : int = 80100

let ct1' = extend id [1;2;3;4]
let ct2' = extend id ("a", 10)
let ct3' = extend id (fun (x:string) -> System.Int32.Parse x)

let ct1'' = (=>>) [1;2;3;4] id
let ct2'' = (=>>) ("a", 10) id
let ct3'' = (=>>) (fun (x:string) -> System.Int32.Parse x) id


// Monoids

type Ordering = LT|EQ|GT with
    static member        MEmpty  (_:Ordering, _:MEmpty) = EQ
    static member        MAppend (x:Ordering, y) = 
        match x, y with
        | LT, _ -> LT
        | EQ, a -> a
        | GT, _ -> GT

let inline compare' x y =
    match compare x y with
    | a when a > 0 -> GT
    | a when a < 0 -> LT
    | _            -> EQ

type Sum<'a> = Sum of 'a with
    static member inline MEmpty() = Sum 0G
    static member inline MAppend (Sum (x:'n), Sum(y:'n)) = Sum (x + y):Sum<'n>

type Product<'a> = Product of 'a with
    static member inline MEmpty() = Product 1G
    static member inline MAppend (Product (x:'n), Product(y:'n)) = Product (x * y):Product<'n>

let inline mempty() = FsControl.Operators.mempty ()
let inline mappend (x:'a) (y:'a): 'a = FsControl.Operators.mappend x y
let inline mconcat (x:seq<'a>) : 'a = FsControl.Operators.mconcat x


// Test Monoids
let emptyLst:list<int> = mempty()
let zeroInt:Sum<int>   = mempty()
let res10 = mappend (mempty()) (Sum 10)
let res6  = mconcat <| fmap Sum [0.4; 5.6]
let res8:Sum<Integer>  = mconcat [mempty(); Sum 2G; Sum 6G]
let res8n4 = [mempty(); [8;4]]
let res15 = mappend (Product 15) (mempty()) 
let resTrue = mconcat [mempty(); Any true]
let resFalse = mconcat (fmap All [true;false])
let resHi = mappend (mempty()) "Hi"
let resGT = mappend (mempty()) GT
let resLT = mconcat [mempty(); LT ; EQ ;GT]
let res9823 = mconcat (fmap Dual [mempty();"3";"2";"8";"9"])
let resBA = (Dual "A" ) </mappend/> (Dual "B" )
let resEl00:list<int>*Sum<float> = mempty()
let resS3P20     = mappend (Sum 1G,Product 5.0) (Sum 2,Product 4G)
let res230       = mappend (mempty(),mempty()) ([2],[3.0])
let res243       = mappend  ([2;4],[3]) (mempty())
let res23        = mappend (mempty()) ([2],"3")
let resLtDualGt  = mappend  (LT,Dual GT) (mempty())
let res230hiSum2 = mappend (mempty(), mempty(), Sum 2) ([2], ([3.0], "hi"), mempty())
let res230hiS4P3 = mappend (mempty(), mempty()       ) ([2], ([3.0], "hi", Sum 4, Product (6 % 2)))
let tuple5 :string*(Any*string)*(All*All*All)*Sum<int>*string = mempty()


// Monad Plus

let inline mzero () = FsControl.Operators.mzero ()
let inline mplus (x:'a) (y:'a) : 'a = FsControl.Operators.(<|>) x y
let inline guard x = if x then return' () else mzero()
type DoPlusNotationBuilder() =
    member inline b.Return(x) = return' x
    member inline b.Bind(p,rest) = p >>= rest
    member b.Let(p,rest) = rest p
    member b.ReturnFrom(expr) = expr
    member inline x.Zero() = mzero()
    member inline x.Combine(a, b) = mplus a b
let doPlus = new DoPlusNotationBuilder()

// Test MonadPlus
let nameAndAddress = mapM (fun x -> putStrLn x >>= fun _ -> getLine) ["name";"address"]

let a:list<int> = mzero()
let res123      = mplus (mempty()) ([1;2;3])

let inline mfilter p ma = do' {
  let! a = ma
  if p a then return a else return! mzero()}

let mfilterRes2 = mfilter ((=)2) (Just 2)

// sample code from http://en.wikibooks.org/wiki/Haskell/MonadPlus
let pythags = do'{
  let! z = [1..50]
  let! x = [1..z]
  let! y = [x..z]
  do! guard (x*x + y*y == z*z)
  return (x, y, z)}

let pythags' = doPlus{
  let! z = [1..50]
  let! x = [1..z]
  let! y = [x..z]
  if (x*x + y*y == z*z) then return (x, y, z)}

let allCombinations = sequence [!"abc"; !"12"]



// Contravariants

module Predicate = let run (p:System.Predicate<_>) x = p.Invoke(x)

let inline contramap (f:'T->'U) (x:'Contravariant'U) :'Contravariant'T = Contramap.Invoke f x

let intToString (x:int) = string x
let resStr54 = contramap (fun (x:float) -> int x) intToString <| 54.
let isEven      = System.Predicate(fun x -> x </mod'/> 2 = 0)
let fstIsEven   = contramap List.head isEven
let resBoolTrue = Predicate.run fstIsEven [0..10]


// BiFunctors

let runKleisli (Kleisli f) = f
let inline bimap f g x = Bimap.Invoke x f g
let inline first   f x = First.Invoke f x
let inline second  f x = Second.Invoke f x

let rInt10Str10 = bimap  int string (10.0, 10)
let resR11      = bimap  string ((+) 1) (Right 10)
let rStrTrue    = first  string (true, 10)
let rStr10      = second string (true, 10)

// Profunctors

let inline dimap f g x = Dimap.Invoke x f g
let inline lmap f x = LMap.Invoke f x
let inline rmap f x = RMap.Invoke f x

let resStrFalse     = dimap int string (Predicate.run isEven) 99.0

// Arrows

let inline id'() = FsControl.Operators.catId ()
let inline (<<<) f g = FsControl.Operators.(<<<<) f g
let inline (>>>) f g = FsControl.Operators.(>>>>) f g
let inline arr   f = FsControl.Operators.arr    f
let inline arrFirst  f = FsControl.Operators.arrFirst f
let inline arrSecond f = FsControl.Operators.arrSecond f
let inline ( *** ) f g = arrFirst f >>> arrSecond g
let inline ( &&& ) f g = arr (fun b -> (b, b)) >>> f *** g
let inline (|||) f g = FsControl.Operators.(||||) f g
let inline (+++) f g = FsControl.Operators.(++++) f g
let inline left  f = FsControl.Operators.left  f
let inline right f = FsControl.Operators.right f
let inline app() = FsControl.Operators.arrApply ()
let inline zeroArrow() = FsControl.Operators.mzero ()
let inline (<+>)   f g = FsControl.Operators.(<|>) f g

// Test Arrows
let r5:List<_>  = (runKleisli (id'())) 5
let k = Kleisli (fun y -> [y; y * 2 ; y * 3]) <<< Kleisli (fun x -> [x + 3; x * 2])
let r8n16n24n10n20n30 = runKleisli k  <| 5
let r20n5n30n5   = runKleisli (arrFirst  <| Kleisli (fun y -> [y * 2; y * 3])) (10,5) 
let r10n10n10n15 = runKleisli (arrSecond <| Kleisli (fun y -> [y * 2; y * 3])) (10,5)

let res3n6n9 = (arr (fun y -> [y; y * 2 ; y * 3])) 3
let resSome2n4n6:option<_> = runKleisli (arr (fun y -> [y; y * 2 ; y * 3])) 2

let res500n19 = ( (*) 100) *** ((+) 9)  <| (5,10)
let res500n14 = ( (*) 100) &&& ((+) 9)  <| 5
let (res10x13n10x20n15x13n15x20:list<_>) = runKleisli (Kleisli (fun y -> [y * 2; y * 3]) *** Kleisli (fun x -> [x + 3; x *  2] )) (5,10)
let (res10x8n10x10n15x8n15x10  :list<_>) = runKleisli (Kleisli (fun y -> [y * 2; y * 3]) &&& Kleisli (fun x -> [x + 3; x *  2] )) 5

// Test Arrow Choice
let resLeft7       = ( (+) 2) +++ ( (*) 10)   <| Left  5
let res7n50        = runKleisli (Kleisli (fun y -> [y; y * 2 ; y * 3]) ||| Kleisli (fun x -> [x + 2; x * 10] )) (Right 5)
let resLeft5n10n15 = runKleisli (Kleisli (fun y -> [y; y * 2 ; y * 3]) +++ Kleisli (fun x -> [x + 3; x *  2] )) (Left  5)

// Test Arrow Apply
let res7      = app() ( (+) 3 , 4)
let res4n8n12 = runKleisli (app()) (Kleisli (fun y -> [y; y * 2 ; y * 3]) , 4)

// Test Arrow Plus
let resSomeX = Kleisli(fun x -> Some x)
let (resSomeXPlusZero:option<_>) = runKleisli (resSomeX <+> zeroArrow()) 10

// Applicative functors

let inline pure' x   = FsControl.Operators.result x
let inline (<*>) x y = FsControl.Operators.(<*>) x y
let inline empty()   = FsControl.Operators.mzero ()
let inline (<|>) x y = FsControl.Operators.(<|>) x y


let inline (<<|>) f a   = fmap f a
let inline liftA2 f a b = f <<|> a <*> b
let inline (  *>)   x   = x |> liftA2 (const' id)
let inline (<*  )   x   = x |> liftA2 const'
let inline (<**>)   x   = x |> liftA2 (|>)

let inline optional v = Just <<|> v <|> pure' Nothing

type ZipList<'s> = ZipList of 's seq with
    static member Map    (ZipList x, f:'a->'b)               = ZipList (Seq.map f x)
    static member Return (x:'a)                              = ZipList (Seq.initInfinite (const' x))
    static member (<*>) (ZipList (f:seq<'a->'b>), ZipList x) = ZipList (Seq.zip f x |> Seq.map (fun (f, x) -> f x)) :ZipList<'b>

// Test Applicative (lists)
let res3n4   = pure' ((+) 2) <*> [1;2]
let res2n4n8 = pure' ( **^) </ap/> pure' 2. <*> [1;2;3]

// Test Applicative (functions)
let res3 = pure' 3 "anything"
let res607 = fmap (+) ( (*) 100 ) 6 7
let res606 = ( (+) <*>  (*) 100 ) 6
let res508 = (fmap (+) ((+) 3 ) <*> (*) 100) 5

// Test Applicative (ZipList)
let res9n5   = fmap ((+) 1) (ZipList(seq [8;4]))
let res18n24 = pure' (+) <*> ZipList(seq [8;4]) <*> ZipList(seq [10;20])
let res6n7n8 = pure' (+) <*> pure' 5G <*> ZipList [1;2;3]
let res18n14 = pure' (+) <*> ZipList(seq [8;4]) <*> pure' 10

// Idiom brackets from http://www.haskell.org/haskellwiki/Idiom_brackets
type Ii = Ii
type Ji = Ji
type J = J
type Idiomatic = Idiomatic with
    static member inline ($) (Idiomatic, si) = fun sfi x -> (Idiomatic $ x) (sfi <*> si)
    static member        ($) (Idiomatic, Ii) = id
let inline idiomatic a b = (Idiomatic $ b) a
let inline iI x = (idiomatic << pure') x

let res3n4''  = iI ((+) 2) [1;2] Ii
let res3n4''' = iI (+) (pure' 2) [1;2] Ii                               // *1
let res18n24' = iI (+) (ZipList(seq [8;4])) (ZipList(seq [10;20])) Ii
let res6n7n8' = iI (+) (pure' 5G          ) (ZipList [1;2;3]     ) Ii   // *1
let res18n14' = iI (+) (ZipList(seq [8;4])) (pure' 10            ) Ii

type Idiomatic with static member inline ($) (Idiomatic, Ji) = fun xii -> join xii

let safeDiv x y = if y == 0 then Nothing else Just (x </div/> y)
let resJust3    = join (iI safeDiv (Just 6) (Just 2) Ii)
let resJust3'   =       iI safeDiv (Just 6) (Just 2) Ji

let safeDivBy y = if y == 0 then Nothing else Just (fun x -> x </div/> y)
let resJust2  = join (pure' safeDivBy  <*> Just 4G) <*> Just 8G
let resJust2' = join (   iI safeDivBy (Just 4G) Ii) <*> Just 8G

type Idiomatic with static member inline ($) (Idiomatic, J ) = fun fii x -> (Idiomatic $ x) (join fii)

let resJust2'' = iI safeDivBy (Just 4G) J (Just 8G) Ii
let resNothing = iI safeDivBy (Just 0G) J (Just 8G) Ii
let res16n17  = iI (+) (iI (+) (pure' 4) [2;3] Ii) (pure'  10) Ii   // *1

// *1 These lines fails when Apply.Invoke has no 'or ^d' constraint.


// Foldable

let inline foldr (f: 'a -> 'b -> 'b) (z:'b) x :'b = FsControl.Operators.foldBack f x z
let inline foldl (f: 'b -> 'a -> 'b) (z:'b) x :'b = FsControl.Operators.fold     f z x
let inline foldMap (f:'T->'Monoid) (x:'Foldable'T) :'Monoid = FsControl.Operators.foldMap f x

// Test Foldable
let resGt = foldMap (compare' 2) [1;2;3]
let resHW = foldMap (fun x -> Just ("hello " + x)) (Just "world")

module FoldableTree =
    type Tree<'a> =
        | Empty 
        | Leaf of 'a 
        | Node of (Tree<'a>) * 'a * (Tree<'a>)

        // add instance for Foldable class
        static member inline FoldMap (t:Tree<_>, f, _:FoldMap) =
            let rec _foldMap x f =
                match x with
                | Empty        -> mempty()
                | Leaf n       -> f n
                | Node (l,k,r) -> mappend (_foldMap l f) (mappend (f k) (_foldMap r f) )
            _foldMap t f
        static member inline FoldBack (x:Tree<_>, f, z, _:FoldBack) = FoldBack.FromFoldMap f z x
        static member inline ToSeq    (x:Tree<_>) = Tree.FoldBack (x, (fun x y -> seq {yield x; yield! y}), Seq.empty, Unchecked.defaultof<FoldBack>)
    
    let myTree = Node (Node (Leaf(1), 6, Leaf(3)), 2 , Leaf(9))
    let resSum21      = foldMap Sum     myTree
    let resProduct324 = foldMap Product myTree
    let res21         = foldr   (+) 0   myTree
    let res21'        = foldl   (+) 0   myTree      // <- Uses the default method.


// Traversable

let inline traverse f t = FsControl.Operators.traverse f t
let inline sequenceA  t = FsControl.Operators.sequenceA t

// Test Traversable
let f x = if x < 200 then [3 - x] else []
let g x = if x < 200 then Just (3 - x) else Nothing

let resSomeminus100 = traverse f (Just 103)
let resLstOfNull    = traverse f Nothing 
let res210          = traverse f [1;2;3]  
let resSome210      = traverse g [1;2;3]  
let resEmptyList    = traverse f [1000;2000;3000] 
let resEListOfElist = traverse f []
let resSome321  = sequenceA [Some 3;Some 2;Some 1]
let resNone     = sequenceA [Some 3;None  ;Some 1]
let res654      = sequenceA [ (+)3 ; (+)2 ; (+) 1] 3
let resCombined = sequenceA [ [1;2;3] ; [4;5;6]  ]
let resLstOfArr = sequenceA [|[1;2;3] ; [4;5;6] |]  // <- Uses the default method.
let resArrOfLst = sequenceA [[|1;2;3|];[|4;5;6 |]]
let get3strings = sequenceA [getLine;getLine;getLine]


// Cont
let runCont = Cont.run
let callCC' = Cont.callCC
let inline when'  p s = if p then s else return' ()
let inline unless p s = when' (not p) s

// Test Cont
let square_C   x = return' (x * x)
let addThree_C x = return' (x + 3)

let res19 = runCont (square_C 4 >>= addThree_C) id

let inline add_cont x y  = return' (x + y)
let inline square_cont x = return' (sqrt x)

let pythagoras_cont x y = do' {
    let! x_squared = square_cont x
    let! y_squared = square_cont y
    let! sum_of_squares = add_cont x_squared y_squared
    return sum_of_squares}

let resPyth373205 = runCont (pythagoras_cont 3. 4.) string

let foo n =
  callCC' <| fun k -> do' {
    let n' = (n * n) + 3
    do! when' (n' > 20) <| k "over twenty"
    return (string <| n' - 4) }

let res''3''  = runCont (foo  2) id
let resOver20 = runCont (foo 16) id


// Reader
let ask'      = Reader.ask
let local'    = Reader.local
let runReader = Reader.run

// Test Reader
let calculateContentLen = do' {
    let! content = ask'()
    return (String.length content)}

let calculateModifiedContentLen = local' ( (+) "Prefix ") calculateContentLen

let readerMain = do' {
    let s = "12345"
    let modifiedLen = runReader calculateModifiedContentLen s
    let len = runReader calculateContentLen s
    do!     putStrLn <| "Modified 's' length: " + (string modifiedLen)
    return! putStrLn <| "Original 's' length: " + (string len)
    }
// try -> runIO readerMain ;;


// Writer
let res12n44x55x1x2 = (+) <<|> Writer (3,[44;55]) </ap/> Writer (9,[1;2])


// State
let runState  = State.run
let get'      = State.get
let put'      = State.put
let execState = State.exec

// from http://www.haskell.org/haskellwiki/State_Monad
let x1 = runState (return' 'X') 1
let xf:State<int,_> = return' 'X'
let r11    = runState (get'()) 1
let rUnit5 = runState (put' 5) 1

let rX5    = runState (do' { 
    do! put' 5
    return 'X' }) 1

let postincrement = do' {
    let! x = get'()
    do! put' (x+1)
    return x }

let r12 = runState postincrement 1

let tick :State<_,_> = do'{
    let! n = get'()
    do! put' (n+1)
    return n}

let plusOne n = execState tick n
let plus  n x = execState (sequence <| List.replicate n tick) x


// Monad Transformers

type MaybeT<'T> = OptionT<'T>
let MaybeT  x = OptionT x
let runMaybeT = OptionT.run
let inline mapMaybeT f x = OptionT.map f x
let runListT  = ListT.run

let inline lift (x:'ma) = FsControl.Operators.lift x
let inline liftIO (x: Async<'a>) = FsControl.Operators.liftAsync x
let inline callCC f = FsControl.Operators.callCC f
let inline get() = FsControl.Operators.get()
let inline put x = FsControl.Operators.put x
let inline ask()     = FsControl.Operators.ask ()
let inline local f m = FsControl.Operators.local f m
let inline tell   x = FsControl.Operators.tell x
let inline listen m = FsControl.Operators.listen m
let inline pass   m = FsControl.Operators.pass   m

// Test Monad Transformers
let maybeT4x6xN = fmap ((+) 2) (MaybeT [Just 2; Just 4; Nothing])
let maybeT2x12x4x14 = MaybeT [Some 2; Some 4] >>= fun x -> MaybeT [Some x; Some (x+10)]

let listT4x6x8  = fmap ((+) 2) (ListT (Just [2; 4; 6]))
let listT2x12x4x14  = ListT  (Some [2;4]    ) >>= fun x -> ListT  (Some [x; x+10]     )

let apMaybeT = ap (MaybeT [Just ((+) 3)] ) ( MaybeT [Just  3 ] )
let apListT  = ap (ListT  (Just [(+) 3]) ) ( ListT  (Just [3]) )

let resListTSome2547 = (ListT (Some [2;4] )) >>=  (fun x -> ListT ( Some [x;x+3G]) )

let getAtLeast8Chars:MaybeT<_> =  lift getLine >>= fun s -> (guard (String.length s >= 8) ) >>= fun _ -> return' s
//try -> runIO <| runMaybeT getAtLeast8Chars

let isValid s = String.length s >= 8 && String.exists System.Char.IsLetter s && String.exists System.Char.IsNumber s && String.exists System.Char.IsPunctuation s

let getValidPassword:MaybeT<_> =
    doPlus {
        let! s = (lift getLine)        
        do! guard (isValid s)  // if isValid s then return s
        return s
        }
    
let askPassword = do' {
    do! lift <| putStrLn "Insert your new password:"
    let! value = getValidPassword
    do! lift <| putStrLn "Storing in database..."
    return value
    }

let askPass = runMaybeT askPassword
//try -> runIO askPass

let resLiftIOMaybeT = liftIO getLine : MaybeT<IO<_>>


// ContT
let runContT  = ContT.run


// from http://en.wikibooks.org/wiki/Haskell/Continuation_passing_style
//askString :: (String -> ContT () IO String) -> ContT () IO String
let askString next = do' {
  do! (liftIO <| putStrLn "Please enter a string") 
  let! s = liftIO <| getLine
  return! next s}

//reportResult :: String -> IO ()
let reportResult s = do' {
  return! putStrLn ("You entered: " + s) }
  
let mainaction = runContT (callCC askString) reportResult
//try -> runIO mainaction


let show x = '\"' :: x ++ !"\""

let inline bar c s = do' {
  let! msg = callCC <| fun k -> do' {
    let s' = c :: s
    do! when' (s' == !"hello") <| k !"They say hello."
    let s'' = show s'
    return (!"They appear to be saying " ++ s'') }
  return (List.length msg) }

let res15'    = runCont            (bar 'h' !"ello")  id
let resSome15 = runCont (runMaybeT (bar 'h' !"ello")) id
let resList29 = runCont (runListT  (bar 'h' !"i"   )) id
let resLiftIOContT = liftIO getLine : ContT<IO<string>,_>


// ReaderT
let runReaderT = ReaderT.run


let res15'' = runCont (runReaderT (bar 'h' !"ello") "anything") id

// from http://www.haskell.org/ghc/docs/6.10.4/html/libraries/mtl/Control-Monad-Reader.html
let printReaderContent = do' {
    let! content = ask()
    return! (liftIO <| putStrLn ("The Reader Content: " + content)) }

let readerTMain = do'{
    return! (runReaderT printReaderContent "Some Content") }

let _ = runIO readerTMain
// try -> runIO readerTMain ;;


// WriterT
let toLower (s:char) = s.ToString().ToLower().Chars(0)
let toUpper (s:char) = s.ToString().ToUpper().Chars(0)

let chncase x = function
    | true -> ((toLower x), false) 
    | _    -> ((toUpper x), true)

let logchncase x = function
    | true -> (((toLower x), "Low "), false)
    | _    -> (((toUpper x), "Up " ), true)
                      
let statecase x = State (logchncase x)
let logstatecase x = WriterT (statecase x)

// runState (runWriterT (logstatecase 'a')) true  -> (char * string) * bool = (('a', "Low "), false)
// runState (runWriterT (logstatecase 'a')) false -> (char * string) * bool = (('A', "Up "), true)

let logstatecase3 x y z : WriterT<_> =  do' {
    let! u = logstatecase x
    let! v = logstatecase y
    let! w = logstatecase z
    do! tell "thats all"
    return [u,v,w]}

//runState (runWriterT (logstatecase3 'a' 'b' 'c')) true  -> ((char * char * char) list * string) * bool = (([('a', 'B', 'c')], "Low Up Low "), false)
//runState (runWriterT (logstatecase3 'a' 'b' 'c')) false -> ((char * char * char) list * string) * bool = (([('A', 'b', 'C')], "Up Low Up "), true)

let resLiftIOWriterT = liftIO getLine : WriterT<IO<_ * string>>


// StateT
let runStateT = StateT.run

// from http://www.haskell.org/haskellwiki/Simple_StateT_use
#nowarn "0025"  // Incomplete pattern match, list cannot be infinite if F#
let code  =
    let inline io (x: IO<_>)  : StateT<_,IO<_>> = liftIO x
    let pop  = do' {
        let! (x::xs) = get()
        do! put xs
        return x}
    do' {
        let! x = pop
        do! io <| print x
        let! y = pop
        do! io <| print y
        return () }

let main = runStateT code [1..10] >>= fun _ -> return' ()

let resLiftIOStateT = liftIO getLine : StateT<string,IO<_>>


// MonadError
let inline throwError x   = FsControl.Operators.throw x
let inline catchError v h = FsControl.Operators.catch v h

// Test MonadError
let err          = throwError "Invalid Value" : MaybeT<Either<_,Maybe<int>>>
let err1Layers   = catchError (Left "Invalid Value") (fun s -> Left ("the error was: " + s) ) : Either<string,int>
let err2Layers   = catchError (MaybeT (Left "Invalid Value")) (fun s -> MaybeT (Left ("the error was: " + s))) : MaybeT<Either<string,Maybe<int>>>
let err3Layers   = catchError (MaybeT(ListT (Left "Invalid Value"))) (fun s -> MaybeT (ListT (Left ("the error was: " + s)))) : MaybeT<ListT<Either<string,List<int>>>>

let err'         = throwError "Invalid Value" : ReaderT<int,Either<_,int>>
let inv = runReaderT err' 5
let err2Layers'   = catchError err' (fun s -> ReaderT (fun x-> Left ("the error was: " + s))) : ReaderT<_,_>
let errWasInv  = runReaderT err2Layers' 5

let err3Layers'  = catchError (MaybeT (throwError "Invalid Value" )) (fun s -> MaybeT(ReaderT (fun x-> Left ("the error was: " + s)))) : OptionT<ReaderT<int,Either<_,Maybe<int>>>>
let err3Layers'' = catchError (ReaderT (fun x -> throwError "Invalid Value" )) (fun s -> ReaderT(fun x-> MaybeT (Left ("the error was: " + s)))) : ReaderT<int,MaybeT<Either<_,Maybe<int>>>>


// ErrorT
let runErrorT = ErrorT.run


let errorT4x6xN = fmap ((+) 2) (ErrorT [Right 2; Right 4; Left "Error"])
let errorT = ErrorT [Right 2; Right 4] >>= fun x -> ErrorT [Right x; Right (x+10)] : ErrorT<List<Either<string,_>>>
let apErrorT = ap (ErrorT [Right ((+) 3)] ) ( ErrorT [Right  3 ] ) : ErrorT<List<Either<string,_>>>
let applyErrorT = ErrorT.apply (ErrorT [Right ((+) 3)] ) ( ErrorT [Right  3 ] ) : ErrorT<List<Either<string,_>>>

let decodeError = function
    | -1 -> "Password not valid"
    | _  -> "Unknown"

let getValidPassword' : ErrorT<_> =
    do' {
        let! s = liftIO getLine
        if isValid s then return s
        else return! throwError -1
        } </catchError/> (fun s -> throwError ("The error was: " + decodeError s))
    
let askPassword' = do' {
    do! lift <| putStrLn "Insert your new password:"
    let! value = getValidPassword'
    do! lift <| putStrLn "Storing in database..."
    return value
    }

let askPass' = runErrorT askPassword'
//try -> runIO askPass'